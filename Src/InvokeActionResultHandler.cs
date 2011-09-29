using System;
using System.Web.Mvc;
using Castle.Core.Interceptor;

namespace MvcTestingHelpers {
	/// <summary>
	/// Handles intercepted calls to ControllerActionInvoker.InvokeActionResult()
	/// </summary>
	public class InvokeActionResultHandler : IInterceptedMethodHandler {
		private readonly Action<ActionResult> callback;

		/// <param name="callback">A callback to invoke when ControllerActionInvoker.InvokeActionResult() is called</param>
		public InvokeActionResultHandler(Action<ActionResult> callback) {
			this.callback = callback;
		}

		public void HandleMethod(IInvocation invocation) {
			callback((ActionResult)invocation.Arguments[1]);
		}

		public string Method { get { return "InvokeActionResult"; } }
	}
}