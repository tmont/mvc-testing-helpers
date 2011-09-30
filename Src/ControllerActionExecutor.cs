using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using Castle.DynamicProxy;
using FakeN.Web;

namespace MvcTestingHelpers {
	/// <summary>
	/// Helper class that enables the full suite of controller invocations in ASP.NET MVC
	/// </summary>
	public class ControllerActionExecutor {
		private readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

		private const HttpVerbs DefaultHttpMethod = HttpVerbs.Get;

		/// <summary>
		/// Executes the given action using the MVC framework, which will execute each of the action filters.
		/// Use this overload only if your controller has a default constructor and you're using the default
		/// ControllerActionInvoker.
		/// </summary>
		/// <param name="actionMethod">The action method on the controller to invoke</param>
		/// <returns>The action result that is returned from the given action method</returns>
		public ActionResult ExecuteActionWithFilters<TController>(Expression<Func<TController, object>> actionMethod) where TController : Controller {
			return ExecuteActionWithFilters(actionMethod, DefaultHttpMethod);
		}

		/// <summary>
		/// Executes the given action using the MVC framework, which will execute each of the action filters.
		/// Use this overload only if your controller has a default constructor and you're using the default
		/// ControllerActionInvoker.
		/// </summary>
		/// <param name="actionMethod">The action method on the controller to invoke</param>
		/// <param name="httpMethod">The HTTP method of the incoming request</param>
		/// <returns>The action result that is returned from the given action method</returns>
		public ActionResult ExecuteActionWithFilters<TController>(Expression<Func<TController, object>> actionMethod, HttpVerbs httpMethod) where TController : Controller {
			return ExecuteActionWithFilters(actionMethod, (TController)Activator.CreateInstance(typeof(TController)), () => new ControllerActionInvoker(), httpMethod);
		}

		/// <summary>
		/// Executes the given action using the MVC framework
		/// </summary>
		/// <param name="actionMethod">The action method on the controller to invoke</param>
		/// <param name="controller">The controller to invoke the action method on</param>
		/// <param name="newInvokerExpression">A factory function to create an action invoker, e.g. () => new ControllerActionInvoker()</param>
		/// <returns>The action result that is returned from the given action method</returns>
		public ActionResult ExecuteActionWithFilters<TController, TInvoker>(Expression<Func<TController, object>> actionMethod, TController controller, Expression<Func<TInvoker>> newInvokerExpression)
			where TController : Controller
			where TInvoker : ControllerActionInvoker {
			return ExecuteActionWithFilters(actionMethod, controller, newInvokerExpression, DefaultHttpMethod);
		}

		/// <summary>
		/// Executes the given action using the MVC framework
		/// </summary>
		/// <param name="actionMethod">The action method on the controller to invoke</param>
		/// <param name="controller">The controller on which to invoke the action method</param>
		/// <param name="newInvokerExpression">The controller action invoker</param>
		/// <param name="httpMethod">The HTTP method of the incoming request</param>
		/// <returns>The action result that is returned from the given action method</returns>
		public ActionResult ExecuteActionWithFilters<TController, TInvoker>(Expression<Func<TController, object>> actionMethod, TController controller, Expression<Func<TInvoker>> newInvokerExpression, HttpVerbs httpMethod)
			where TController : Controller
			where TInvoker : ControllerActionInvoker {
			return ExecuteActionWithFilters(actionMethod, controller, newInvokerExpression, httpMethod, null);
		}

		/// <summary>
		/// Executes the given action using the MVC framework with the given ControllerContext
		/// instead of creating one internally
		/// </summary>
		/// <param name="actionMethod">The action method on the controller to invoke</param>
		/// <param name="controller">The controller on which to invoke the action method</param>
		/// <param name="newInvokerExpression">A factory function to create an action invoker</param>
		/// <param name="httpMethod">The HTTP method that the AcceptVerbs attribute expects</param>
		/// <param name="controllerContext">The ControllerContext to attach to the controller</param>
		/// <returns>The action result that is returned from the given action method</returns>
		public ActionResult ExecuteActionWithFilters<TController, TInvoker>(Expression<Func<TController, object>> actionMethod, TController controller, Expression<Func<TInvoker>> newInvokerExpression, HttpVerbs httpMethod, ControllerContext controllerContext)
			where TController : Controller
			where TInvoker : ControllerActionInvoker {

			VerifyActionExpression(actionMethod);
			var actionName = GetActionNameFromExpression(actionMethod);
			var parametersDictionary = GetParametersFromExpression(actionMethod);

			ActionResult actionResult = null;
			var interceptor = new ControllerActionInvokerInterceptor(
				new IInterceptedMethodHandler[] { 
					new GetParameterValuesHandler(parametersDictionary), 
					new InvokeActionResultHandler(result => actionResult = result) 
				}
			);

			object[] parametersForMockInvoker = GetParametersForMockInvoker(newInvokerExpression);

			var invoker = (TInvoker)proxyGenerator.CreateClassProxy(typeof(TInvoker), parametersForMockInvoker, interceptor);

			controller.ControllerContext = controllerContext ?? CreateControllerContext(controller, httpMethod);
			controller.ActionInvoker = invoker;
			if (!controller.ActionInvoker.InvokeAction(controller.ControllerContext, actionName)) {
				throw new InvalidOperationException(string.Format("Unknown action method \"{0}\" for HTTP method {1}", actionName, httpMethod.ToString().ToUpper()));
			}

			return actionResult;
		}

		/// <summary>
		/// Creates a ControllerContext for the controller
		/// </summary>
		protected virtual ControllerContext CreateControllerContext<TController>(TController controller, HttpVerbs httpMethod) where TController : Controller {
			return new ControllerContext(CreateHttpContext(httpMethod), new RouteData(), controller);
		}

		/// <summary>
		/// Creates an HttpContext for the ControllerContext
		/// </summary>
		protected virtual HttpContextBase CreateHttpContext(HttpVerbs httpMethod) {
			return new FakeHttpContext(CreateHttpRequest(httpMethod), CreateHttpResponse(), CreateHttpSession());
		}

		/// <summary>
		/// Creates an HttpSession for the HttpContext
		/// </summary>
		protected virtual HttpSessionStateBase CreateHttpSession() {
			return new FakeHttpSessionState();
		}

		/// <summary>
		/// Creates an HttpRequest for the HttpContext
		/// </summary>
		protected virtual HttpRequestBase CreateHttpRequest(HttpVerbs httpMethod) {
			return new FakeHttpRequest(method: httpMethod.ToString());
		}

		/// <summary>
		/// Creates an HttpResponse for the HttpContext
		/// </summary>
		protected virtual HttpResponseBase CreateHttpResponse() {
			return new FakeHttpResponse();
		}

		private static IDictionary<string, object> GetParametersFromExpression<TController>(Expression<Func<TController, object>> expression) where TController : Controller {
			var methodCall = (MethodCallExpression)expression.Body;
			var parameters = new Dictionary<string, object>();

			var methodParams = methodCall.Method.GetParameters();
			for (var i = 0; i < methodParams.Length; i++) {
				var paramName = methodParams[i].Name;
				var param = methodCall.Arguments[i];
				parameters[paramName] = Expression.Lambda(param).Compile().DynamicInvoke();
			}

			return parameters;
		}

		private static void VerifyActionExpression<TController>(Expression<Func<TController, object>> expression) where TController : Controller {
			if (expression.Parameters == null || expression.Parameters.Count != 1 || expression.Parameters[0].Type != typeof(TController) || !(expression.Body is MethodCallExpression)) {
				throw new InvalidOperationException("Expression must have one parameter which is the controller and invoke a method on the controller: e.g. controller => controller.MyActionMethod()");
			}
		}

		private static string GetActionNameFromExpression<TController>(Expression<Func<TController, object>> expression) where TController : Controller {
			return ((MethodCallExpression)expression.Body).Method.Name;
		}

		private static object[] GetParametersForMockInvoker<TInvoker>(Expression<Func<TInvoker>> expression) where TInvoker : ControllerActionInvoker {
			var newExpression = expression.Body as NewExpression;
			if (newExpression == null) {
				//cannot infer arguments
				return new object[0];
			}

			return newExpression
				.Arguments
				.Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke())
				.ToArray();
		}
	}
}