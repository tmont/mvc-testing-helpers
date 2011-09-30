using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace MvcTestingHelpers {
	/// <summary>
	/// NUnit extensions for ActionResult
	/// </summary>
	public static class ActionResultExtensions {

		#region A crappy NUnit-compatible assertion framework to remove the NUnit dependency
		/// <summary>
		/// Thrown when one of the assertion extensions fails
		/// </summary>
		public class AssertionException : Exception {
			/// <param name="message">The failure message</param>
			public AssertionException(string message) : base(message) { }
		}

		private static class Assert {
			public static void That(object actual, IConstraint constraint, string message) {
				if (!constraint.IsValid(actual)) {
					throw new AssertionException(message);
				}
			}

			public static void That(bool result, string message) {
				if (!result) {
					throw new AssertionException(message);
				}
			}
		}

		private interface IConstraint {
			bool IsValid(object actual);
			object Expected { get; set; }
		}

		private class EqualToConstraint : IConstraint {
			public bool IsValid(object actual) {
				if (actual == null) {
					return Expected == null;
				}

				if (Expected == null) {
					return false;
				}

				var expectedType = Expected.GetType();
				var actualType = actual.GetType();
				if (expectedType != actualType) {
					return false;
				}

				return Expected.Equals(actual);
			}

			public object Expected { get; set; }
		}

		private class GreaterThanOrEqualToConstraint : IConstraint {
			public bool IsValid(object actual) {
				return (int)actual >= (int)Expected;
			}

			public object Expected { get; set; }
		}

		private class InstanceOfConstraint : IConstraint {
			public bool IsValid(object actual) {
				return actual != null && ((Type)Expected).IsInstanceOfType(actual);
			}

			public object Expected { get; set; }
		}

		private static class Is {
			public static EqualToConstraint EqualTo(object expected) {
				return new EqualToConstraint { Expected = expected };
			}

			public static GreaterThanOrEqualToConstraint GreaterThanOrEqualTo(int expected) {
				return new GreaterThanOrEqualToConstraint { Expected = expected };
			}

			public static InstanceOfConstraint InstanceOf<T>() {
				return new InstanceOfConstraint { Expected = typeof(T) };
			}
		}
		#endregion

		/// <summary>
		/// Gets the strongly typed model from the view data
		/// </summary>
		public static T GetModel<T>(this ActionResult result) {
			var model = result.As<ViewResult>().ViewData.Model;
			Assert.That(model, Is.InstanceOf<T>(), "ViewData.Model is not of type " + typeof(T).Name);
			return (T)model;
		}

		/// <summary>
		/// Gets the action route value from the RedirectToRouteResult
		/// </summary>
		public static string GetRedirectAction(this ActionResult result) {
			var routeValues = result.As<RedirectToRouteResult>().RouteValues;
			Assert.That(routeValues.ContainsKey("action"), "Route values dictionary does not contain a key for \"action\"");
			return routeValues["action"].ToString();
		}

		/// <summary>
		/// Asserts that the model state contains an Exception of type TException
		/// </summary>
		/// <param name="result">The action result</param>
		/// <param name="key">The model state key</param>
		/// <param name="index">The error index</param>
		public static void AssertModelStateError<TException>(this ActionResult result, string key, int index) where TException : Exception {
			var modelState = result.As<ViewResult>().ViewData.ModelState;

			Assert.That(modelState.ContainsKey(key), "Model state does not contain key \"" + key + "\"");
			Assert.That(modelState[key].Errors.Count, Is.GreaterThanOrEqualTo(index + 1), "The model state does not contain an error at index " + index);
			Assert.That(modelState[key].Errors[index].Exception, Is.InstanceOf<TException>(), string.Format("Error at index {0} is not an instanceof {1}", index, typeof(TException).Name));
		}

		/// <summary>
		/// Asserts that the model state contains an error with the specified message
		/// </summary>
		/// <param name="result">The action result</param>
		/// <param name="key">The model state key</param>
		/// <param name="message">The error message</param>
		/// <param name="index">The error index</param>
		public static void AssertModelStateError(this ActionResult result, string key, int index, string message) {
			var modelState = result.As<ViewResult>().ViewData.ModelState;

			Assert.That(modelState.ContainsKey(key), "Model state does not contain key \"" + key + "\"");
			Assert.That(modelState[key].Errors.Count, Is.GreaterThanOrEqualTo(index + 1), "The model state does not contain an error at index " + index);
			Assert.That(modelState[key].Errors[index].ErrorMessage, Is.EqualTo(message), string.Format("Error messages do not match\nExpected: {0}\nActual:   {1}", message, modelState[key].Errors[index].ErrorMessage));
		}

		/// <summary> 
		/// Asserts that the redirect route's route values are correct. If a null value is needed, use (object)null. 
		/// </summary>
		/// <param name="result">The action result</param>
		/// <param name="expectedRouteValues">e.g. new { controller = "Home", action = "Index" }</param>
		public static void AssertRouteValues(this ActionResult result, object expectedRouteValues) {
			var actualRouteValues = result.As<RedirectToRouteResult>().RouteValues;

			const string message = "The route value with index \"{0}\" was not equal to the expected route value: was {1} but got {2}";
			foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(expectedRouteValues)) {
				var actual = actualRouteValues[prop.Name];
				var expected = prop.GetValue(expectedRouteValues);
				Assert.That(actual, Is.EqualTo(expected), string.Format(message, prop.Name, actual, expected));
			}
		}

		/// <summary>
		/// Performs a safe cast using assertions to the specified ActionResult type
		/// </summary>
		public static T As<T>(this ActionResult result) where T : ActionResult {
			Assert.That(result, Is.InstanceOf<T>(), string.Format("Unable to cast an instance of {0} to type {1}", result == null ? "null" : result.GetType().Name, typeof(T).Name));
			return (T)result;
		}
	}
}