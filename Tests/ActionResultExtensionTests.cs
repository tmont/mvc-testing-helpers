using System;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;

namespace MvcTestingHelpers.Tests {
	[TestFixture]
	public class ActionResultExtensionTests {

		[Test]
		public void Should_cast_to_appropriate_ActionResult_type() {
			ActionResult result = new ViewResult();
			Assert.That(result.As<ViewResult>(), Is.InstanceOf<ViewResult>());
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Unable to cast an instance of ViewResult to type RedirectResult")]
		public void Should_throw_if_unable_to_cast_to_ActionResult_type() {
			new ViewResult().As<RedirectResult>();
		}

		[Test]
		public void Should_verify_all_route_values() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Foo", action = "Index", id = 1 }));
			result.AssertRouteValues(new { controller = "Foo", action = "Index", id = 1 });
		}

		[Test]
		public void Should_verify_route_values_but_ignore_ones_not_given() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Foo", action = "Index", id = 1 }));
			result.AssertRouteValues(new { action = "Index", id = 1 });
		}

		[Test]
		public void Should_verify_null_route_values_correctly() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = (object)null }));
			result.AssertRouteValues(new { controller = (string)null });
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "The route value with index \"controller\" was not equal to the expected route value: was Foo but got Bar")]
		public void Should_throw_if_route_values_do_not_match() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Foo" }));
			result.AssertRouteValues(new { controller = "Bar" });
		}

		[Test]
		public void Should_get_model() {
			var model = new Exception();
			var result = new ViewResult { ViewData = new ViewDataDictionary { Model = model } };

			Assert.That(result.GetModel<Exception>(), Is.EqualTo(model));
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "ViewData.Model is not of type ArgumentException")]
		public void Should_blow_up_if_model_is_not_of_expected_type() {
			var result = new ViewResult { ViewData = new ViewDataDictionary { Model = new Exception() } };

			result.GetModel<ArgumentException>();
		}

		[Test]
		public void Should_get_action_from_redirect_result() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "foo" }));
			Assert.That(result.GetRedirectAction(), Is.EqualTo("foo"));
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Route values dictionary does not contain a key for \"action\"")]
		public void Should_blow_up_if_redirect_result_does_contain_an_action() {
			var result = new RedirectToRouteResult(new RouteValueDictionary(new { lulz = "foo" }));
			result.GetRedirectAction();
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Model state does not contain key \"foo\"")]
		public void Should_blow_up_if_model_state_does_not_have_key() {
			var viewDataDictionary = new ViewDataDictionary();

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError("foo", 0, "foo");
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "The model state does not contain an error at index 1")]
		public void Should_blow_up_if_model_state_does_not_have_error_at_specified_index() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", "error message");

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError("foo", 1, "foo");
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Error messages do not match\nExpected: not the correct message\nActual:   error message")]
		public void Should_blow_up_if_model_state_error_message_do_not_match() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", "error message");

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError("foo", 0, "not the correct message");
		}

		[Test]
		public void Should_verify_model_error_message() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", "error message");

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError("foo", 0, "error message");
		}


		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Model state does not contain key \"foo\"")]
		public void Should_blow_up_if_model_state_does_not_have_key_for_exception() {
			var viewDataDictionary = new ViewDataDictionary();

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError<Exception>("foo", 0);
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "The model state does not contain an error at index 1")]
		public void Should_blow_up_if_model_state_does_not_have_exception_at_specified_index() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", new Exception());

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError<Exception>("foo", 1);
		}

		[Test]
		[ExpectedException(typeof(ActionResultExtensions.AssertionException), ExpectedMessage = "Error at index 0 is not an instanceof ArgumentNullException")]
		public void Should_blow_up_if_model_state_exceptions_do_not_match() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", new ArgumentException());

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError<ArgumentNullException>("foo", 0);
		}

		[Test]
		public void Should_verify_model_state_exception() {
			var viewDataDictionary = new ViewDataDictionary();
			viewDataDictionary.ModelState.AddModelError("foo", new ArgumentException());

			var result = new ViewResult {
				ViewData = viewDataDictionary
			};

			result.AssertModelStateError<ArgumentException>("foo", 0);
		}

	}
}