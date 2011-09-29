using Castle.Core.Interceptor;

namespace MvcTestingHelpers {
	/// <summary>
	/// Handler for methods intercepted by the dynamic proxy
	/// </summary>
	public interface IInterceptedMethodHandler {
		/// <summary>
		/// Performs any needed actions for the specified method invocation
		/// </summary>
		/// <param name="invocation"></param>
		void HandleMethod(IInvocation invocation);

		/// <summary>
		/// The name of the method to handle
		/// </summary>
		string Method { get; }
	}
}