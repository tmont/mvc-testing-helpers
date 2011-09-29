using System;
using System.Web.Mvc;
using NUnit.Framework;

namespace MvcTestingHelpers.Tests {
	[TestFixture]
	public class ControllerActionExecutorTests {

		private readonly ControllerActionExecutor executor = new ControllerActionExecutor();

		[Test]
		public void Should_call_post_action() {
			var result = executor.ExecuteActionWithFilters(
				c => c.PostActionMethod("hello world"), 
				new MyController("foo"),
				() => new MyActionInvoker(), 
				HttpVerbs.Post
			);

			Assert.That(result.GetModel<MyModel>().HelloWorld, Is.EqualTo("hello world"));
		}

		[Test]
		public void Should_properly_instantiate_controller() {
			var result = executor.ExecuteActionWithFilters(c => c.PostActionMethod(null), new MyController("foo"), () => new MyActionInvoker(), HttpVerbs.Post);
			Assert.That(result.GetModel<MyModel>().HelloWorld, Is.EqualTo("foo"));
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unknown action method \"PostActionMethod\" for HTTP method GET")]
		public void Should_call_post_action_with_get_and_fail() {
			executor.ExecuteActionWithFilters(c => c.PostActionMethod("hello world"), new MyController("foo"), () => new MyActionInvoker());
		}

		[Test]
		public void Should_execute_custom_action_filter() {
			var result = executor.ExecuteActionWithFilters(c => c.ActionWithFilter(), new MyController("foo"), () => new MyActionInvoker());
			Assert.That(result.As<ContentResult>().Content, Is.EqualTo("action with filter"));
			Assert.That(MyFilter.InvocationCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_use_activator_to_create_controller_and_action_invoker() {
			var result = executor.ExecuteActionWithFilters<MyController>(c => c.Index());
			Assert.That(result, Is.InstanceOf<ViewResult>());
		}

		#region Mocks
		internal class MyController : Controller {
			private readonly string defaultText;

			public MyController() {
				defaultText = string.Empty;
			}

			public MyController(string defaultText) {
				this.defaultText = defaultText;
			}

			[AcceptVerbs(HttpVerbs.Post)]
			public ActionResult PostActionMethod(string text) {
				return View(new MyModel { HelloWorld = text ?? defaultText });
			}

			[MyFilter]
			public string ActionWithFilter() {
				return "action with filter";
			}

			public ViewResult Index() {
				return View();
			}

		}

		public class MyFilter : ActionFilterAttribute {

			public static int InvocationCount { get; private set; }

			public override void OnActionExecuting(ActionExecutingContext filterContext) {
				InvocationCount++;
			}
		}

		public class MyActionInvoker : ControllerActionInvoker { }
		internal class InternalActionInvoker : ControllerActionInvoker { }

		internal class MyModel {
			public string HelloWorld { get; set; }
		}
		#endregion

	}


}
