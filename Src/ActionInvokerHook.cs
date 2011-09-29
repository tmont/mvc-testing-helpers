using System;
using System.Reflection;
using Castle.DynamicProxy;
using System.Linq;

namespace MvcTestingHelpers {
	/// <summary>
	/// Hook for ControllerActionInvoker proxies; tells the proxy generator to intercept
	/// calls to InvokeActionResult() and GetParameterValues()
	/// </summary>
	public class ActionInvokerHook : IProxyGenerationHook {

		private static readonly string[] methodsToIntercept = new[] { "InvokeActionResult", "GetParameterValues" };

		public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo) {
			return methodsToIntercept.Contains(methodInfo.Name);
		}

		#region Methods we don't care about but are forced to implement
		public void NonVirtualMemberNotification(Type type, MemberInfo memberInfo) { }
		public void MethodsInspected() { }
		#endregion
	}
}