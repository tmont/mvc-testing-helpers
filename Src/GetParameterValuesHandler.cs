using System.Collections.Generic;
using Castle.Core.Interceptor;

namespace MvcTestingHelpers {
	/// <summary>
	/// Handles intercepted calls to ControllerActionInvoker.GetParameterValues()
	/// </summary>
	public class GetParameterValuesHandler : IInterceptedMethodHandler {
		private readonly IDictionary<string, object> parameters;

		/// <param name="parameters">The method arguments given to the controller's action method invocation</param>
		public GetParameterValuesHandler(IDictionary<string, object> parameters) {
			this.parameters = parameters;
		}

		public void HandleMethod(IInvocation invocation) {
			invocation.ReturnValue = parameters;
		}

		public string Method {
			get { return "GetParameterValues"; }
		}
	}
}