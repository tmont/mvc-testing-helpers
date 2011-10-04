using System.Collections.Generic;
using System.Linq;
using Castle.Core.Interceptor;

namespace MvcTestingHelpers {
	/// <summary>
	/// Method interceptor for proxies of ControllerActionInvoker
	/// </summary>
	public class ControllerActionInvokerInterceptor : IInterceptor {
		/// <summary>
		/// The method handlers associated with this interceptor
		/// </summary>
		public IEnumerable<IInterceptedMethodHandler> MethodHandlers { get; private set; }

		///<summary/>
		public ControllerActionInvokerInterceptor(IEnumerable<IInterceptedMethodHandler> methodHandlers) {
			MethodHandlers = methodHandlers ?? Enumerable.Empty<IInterceptedMethodHandler>();
		}

		/// <summary>
		/// Invokes the appropriate method handler for the given invocation
		/// </summary>
		public void Intercept(IInvocation invocation) {
			var handler = MethodHandlers
				.SingleOrDefault(methodHandler => methodHandler.Method == invocation.Method.Name);

			if (handler != null) {
				handler.HandleMethod(invocation);
			} else {
				invocation.Proceed();
			}
		}
	}
}