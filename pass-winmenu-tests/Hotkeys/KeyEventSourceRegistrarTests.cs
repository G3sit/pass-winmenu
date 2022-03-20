using System;
using System.Linq;
using System.Windows.Input;
using PassWinmenu.Hotkeys;
using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.Hotkeys
{
		public class KeyEventSourceRegistrarTests
	{
		private readonly DummyKeyEventSource _dummyEventSource;

		public KeyEventSourceRegistrarTests()
		{
			_dummyEventSource = new DummyKeyEventSource();
			_registrar = HotkeyRegistrars.KeyEventSource.Create(_dummyEventSource);
		}


		private IHotkeyRegistrar _registrar;

		[StaFact]
		public void Create_IKeyEventSource_ThrowsOnNullSource()
		{
			Assert.Throws<ArgumentNullException>(
				() => HotkeyRegistrars.KeyEventSource.Create(null)
				);
		}

		[StaFact]
		public void Create_TSource_ThrowsOnNullSource()
		{
			Assert.Throws<ArgumentNullException>(
				() => HotkeyRegistrars.KeyEventSource.Create<object>(
					null, o => throw new InvalidOperationException()
					)
				);
		}
		[StaFact]
		public void Create_TSource_ThrowsOnNullAdaptor()
		{
			Assert.Throws<ArgumentNullException>(
				() => HotkeyRegistrars.KeyEventSource.Create<object>(
					new object(), null
					)
				);
		}
		[StaFact]
		public void Create_TSource_ThrowsOnFailedAdaptation()
		{
			Assert.Throws<ArgumentException>(
				() => HotkeyRegistrars.KeyEventSource.Create<object>(
					new object(), o => null
					)
				);
		}


		[StaFact]
		public void Register_ThrowsOnNullHandler()
		{
			Assert.Throws<ArgumentNullException>(
				() => _registrar.Register(
					modifierKeys: ModifierKeys.Control,
					key:          Key.P,
					repeats:      false,
					firedHandler: null
					)
				);
		}

		[StaFact]
		public void Register_KeyEquivalence_Control()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Control,
				key:            Key.A, 
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			var left  = new[] { Key.LeftCtrl,  Key.A };
			var right = new[] { Key.RightCtrl, Key.A };

			_dummyEventSource.Actuate(left);
			_dummyEventSource.Release(left);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(right);
			_dummyEventSource.Release(right);

			Assert.Equal(2, firedCount);
		}
		[StaFact]
		public void Register_KeyEquivalence_Shift()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Alt,
				key:            Key.X, 
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			var left  = new[] { Key.LeftAlt,  Key.X };
			var right = new[] { Key.RightAlt, Key.X };

			_dummyEventSource.Actuate(left);
			_dummyEventSource.Release(left);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(right);
			_dummyEventSource.Release(right);

			Assert.Equal(2, firedCount);
		}
		[StaFact]
		public void Register_KeyEquivalence_Alt()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Shift,
				key:            Key.M, 
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			var left  = new[] { Key.LeftShift,  Key.M };
			var right = new[] { Key.RightShift, Key.M };

			_dummyEventSource.Actuate(left);
			_dummyEventSource.Release(left);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(right);
			_dummyEventSource.Release(right);

			Assert.Equal(2, firedCount);
		}

		[StaFact]
		public void Register_CombinationSimple_NoRepeat()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Control,
				key:            Key.A, 
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			var keys = new[] { Key.LeftCtrl, Key.A };

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Release(keys);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Actuate(keys, isRepeat: true);
			_dummyEventSource.Release(keys);

			// Should ignore repeat actuations
			Assert.Equal(2, firedCount);

			deregisterer.Dispose();

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);
		}
		[StaFact]
		public void Register_CombinationSimple_Repeats()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Alt,
				key:            Key.X,
				repeats:        true,
				firedHandler:   (s, e) => firedCount++
				);

			var keys = new[] { Key.RightAlt, Key.X };

			// Actuate multiple times, indicating that the keys are repeats
			// after the first time, to emulate a held-down key combo.

			Assert.Equal(0, firedCount);

			_dummyEventSource.Actuate(keys, isRepeat: false);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(keys, isRepeat: true);

			Assert.Equal(2, firedCount);

			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);

			deregisterer.Dispose();

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Actuate(keys, isRepeat: true);
			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);
		}

		[StaFact]
		public void Register_CombinationComplex_NoRepeat()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Control | ModifierKeys.Alt,
				key:            Key.A, 
				repeats:        false,
				firedHandler:   (s, e) =>
				{
					firedCount++;
				});

			var keys = new[] { Key.LeftCtrl, Key.RightAlt, Key.A };

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Release(keys);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Actuate(keys, isRepeat: true);
			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);

			deregisterer.Dispose();

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Actuate(keys, isRepeat: true);
			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);
		}
		[StaFact]
		public void Register_CombinationComplex_Repeats()
		{
			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift,
				key:            Key.X, 
				repeats:        true,
				firedHandler:   (s, e) => firedCount++
				);

			var keys = new[] { Key.RightCtrl, Key.LeftShift, Key.X };

			_dummyEventSource.Actuate(keys, isRepeat: false);

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(keys, isRepeat: true);

			Assert.Equal(2, firedCount);

			_dummyEventSource.Release(keys);

			Assert.Equal(2, firedCount);

			deregisterer.Dispose();

			_dummyEventSource.Actuate(keys, isRepeat: false);

			Assert.Equal(2, firedCount);

			_dummyEventSource.Actuate(keys, isRepeat: true);

			Assert.Equal(2, firedCount);
		}

		[StaFact]
		public void Register_UnreliableActuation()
		{
			// Unreliable actuation, i.e. where the user would actuate keys,
			// release some of them, then actuate again to finally make the
			// full key combination.
			//
			// Key repeats are thrown in to emulate a key being continuously
			// held down before the combination is fully made.

			int firedCount = 0;

			_registrar.Register(
				modifierKeys: ModifierKeys.Control | ModifierKeys.Alt,
				key: Key.A,
				repeats: false,
				firedHandler: (s, e) => firedCount++
				);

			_dummyEventSource.Actuate(Key.LeftCtrl);

			Assert.Equal(0, firedCount);

			_dummyEventSource.Actuate(Key.LeftAlt);
			_dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

			Assert.Equal(0, firedCount);

			_dummyEventSource.Release(Key.LeftAlt);
			_dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

			Assert.Equal(0, firedCount);

			_dummyEventSource.Actuate(Key.LeftCtrl, isRepeat: true);

			Assert.Equal(0, firedCount);

			_dummyEventSource.Actuate(new[] { Key.LeftAlt, Key.A });

			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(
				new[] { Key.LeftCtrl, Key.LeftAlt, Key.A }, isRepeat: true
				);

			Assert.Equal(1, firedCount);
		}

		[StaFact]
		public void Register_IrrelevantKeys()
		{
			// Other keys may be pressed while the combination is actuated. A hotkey
			// should ignore these keys and not fire unless the combination is fully
			// pressed.

			int fireCount = 0;

			_registrar.Register(
				ModifierKeys.Control, Key.A, true, (s, e) => fireCount++
				);

			_dummyEventSource.Actuate(new[] { Key.LeftCtrl, Key.F, Key.LeftShift, Key.A, Key.R });

			// Fires initially with noise
			Assert.Equal(1, fireCount);

			_dummyEventSource.Actuate(new[] { Key.F, Key.LeftCtrl, Key.R, Key.A }, isRepeat: true);

			// Fires subsequently with noise
			Assert.Equal(2, fireCount);

			_dummyEventSource.Actuate(new[] { Key.LeftShift, Key.LeftCtrl }, isRepeat: true);

			// Continues not to fire, even with irrelevant keys, because of wrong key
			// actuation order (modifiers first, then key)
			Assert.Equal(2, fireCount);

			_dummyEventSource.Actuate(new[] { Key.Escape, Key.A, Key.Q }, isRepeat: true);

			// Fires, even with irrelevant keys, once the modifiers + key have been
			// actuated in the correct order
			Assert.Equal(3, fireCount);
		}

		[StaTheory]
		[InlineData(true)]
		[InlineData(false)]
		public void Register_HeldModifiers(bool isRepeat)
		{
			// For consistency with the Windows hotkey API, we want to trigger
			// even if only the final key (and not any modifier key) is released,
			// for example:
			//
			//      If the combination is [Ctrl+A], holding [Ctrl] down and
			//      then repeatedly actuating and releasing [A] should trigger
			//      the hotkey, ie:
			//
			//          Ctrl A A A A
			//
			//      Should result in the hotkey being triggered four times.
			//
			// This should occur irrespective of whether the hotkey is configured
			// to fire repeatedly on continuous key actuation.
			//
			// It should also occur if the modifiers are released and then actuated
			// again, for example:
			//
			//      If the combination is [Ctrl+Shift+A], then:
			//
			//          Ctrl Shift A A A
			//          Ctrl Shift A Shift A Shift A
			//
			//      Should all result in the hotkey being triggered three times.

			int firedCount = 0;

			var deregisterer = _registrar.Register(
				modifierKeys:   ModifierKeys.Alt | ModifierKeys.Shift,
				key:            Key.X,
				repeats:        isRepeat,
				firedHandler:   (s, e) => firedCount++
				);


			// First actuation
			_dummyEventSource.Actuate(
				new[] { Key.RightAlt, Key.LeftShift, Key.X }, isRepeat: false
				);

			Assert.Equal(1, firedCount);

			// Release, re-actuate final key
			_dummyEventSource.Release(Key.X);
			_dummyEventSource.Actuate(
				new[] { Key.RightAlt, Key.LeftShift }, isRepeat: true
				);
			_dummyEventSource.Actuate(Key.X, isRepeat: false);

			Assert.Equal(2, firedCount);

			// Further repeated release, re-actuates
			_dummyEventSource.Release(Key.X);
			_dummyEventSource.Actuate(Key.X); // +1
			_dummyEventSource.Release(Key.X);
			_dummyEventSource.Actuate(Key.X); // +1
			_dummyEventSource.Release(Key.X);

			Assert.Equal(4, firedCount);


			// Release, re-actuate modifier and final key
			_dummyEventSource.Release(new[] { Key.LeftShift, Key.X });
			_dummyEventSource.Actuate(Key.RightAlt, isRepeat: true);
			_dummyEventSource.Actuate(new[] { Key.LeftShift, Key.X });

			Assert.Equal(5, firedCount);


			// Release, re-actuate final key and different modifier
			_dummyEventSource.Release(new[] { Key.RightAlt, Key.X });
			_dummyEventSource.Actuate(Key.LeftShift, isRepeat: true);
			_dummyEventSource.Actuate(new[] { Key.RightAlt, Key.X });

			Assert.Equal(6, firedCount);


			// Actuation order still enforced
			_dummyEventSource.Release(new[] { Key.LeftShift, Key.X });
			_dummyEventSource.Actuate(new[] { Key.X, Key.LeftShift });

			Assert.Equal(6, firedCount);

			_dummyEventSource.Release(new[] { Key.RightAlt, Key.X });
			_dummyEventSource.Actuate(new[] { Key.X, Key.RightAlt });

			Assert.Equal(6, firedCount);
		}

		// The Windows hotkey registrar cannot accept two hotkeys with the same
		// combination where one repeats and the other doesn't. While there isn't
		// anything preventing this with the [KeyEventSource] registrar, we prevent
		// it for consistency.

		[StaFact]
		public void Register_MultipleHandlers_AllowedHomogeneous_NoRepeat()
		{
			string firedString = String.Empty;

			// Works with no-repeat
			var deregA = _registrar.Register(
				modifierKeys:   ModifierKeys.Alt,
				key:            Key.R,
				repeats:        false,
				firedHandler:   (s, e) => firedString += "A"
				);

			var deregB = _registrar.Register(
				modifierKeys:   ModifierKeys.Alt,
				key:            Key.R,
				repeats:        false,
				firedHandler:   (s, e) => firedString += "B"
				);

			_dummyEventSource.Actuate(new[] { Key.LeftAlt, Key.R });

			Assert.Equal(1, firedString.Count(c => c == 'A'));
			Assert.Equal(1, firedString.Count(c => c == 'B'));
		}
		[StaFact]
		public void Register_MultipleHandlers_AllowedHomogeneous_Repeats()
		{
			string firedString = String.Empty;

			// Works with no-repeat
			var deregA = _registrar.Register(
				modifierKeys:   ModifierKeys.Shift,
				key:            Key.Y,
				repeats:        true,
				firedHandler:   (s, e) => firedString += "A"
				);

			var deregB = _registrar.Register(
				modifierKeys:   ModifierKeys.Shift,
				key:            Key.Y,
				repeats:        true,
				firedHandler:   (s, e) => firedString += "B"
				);

			_dummyEventSource.Actuate(new[] { Key.RightShift, Key.Y });

			Assert.Equal(1, firedString.Count(c => c == 'A'));
			Assert.Equal(1, firedString.Count(c => c == 'B'));
		}
		[StaFact]
		public void Register_MultipleHandlers_FailsWithMixedRepeat()
		{
			// First doesn't repeat
			_registrar.Register(ModifierKeys.Shift, Key.F, false, (s, e) => { });

			// Second does
			Assert.Throws<HotkeyException>(() =>
			{
				_registrar.Register(ModifierKeys.Shift, Key.F, true, (s, e) => { });
			});
		}

		// The registrar should enforce that the combination is only
		// triggered when the keys are pressed in order.
		//
		// The correct order is modifier keys first, followed by the
		// regular key. The order of the modifier keys is ignored. This
		// is the behaviour confirmed by experiment for the Windows
		// hotkey registrar.

		[StaFact]
		public void Register_EnforceCombinationOrder_Basic()
		{
			// Basic test, correct order works and wrong order doesn't with a
			// simple combination.

			int firedCount = 0;

			_registrar.Register(
				ModifierKeys.Control, Key.A, repeats: false,
				firedHandler: (s, e) => firedCount++
				);

			var keys = new[] { Key.LeftCtrl, Key.A };

			_dummyEventSource.Actuate(keys);
			_dummyEventSource.Release(keys);

			// Correct order triggers
			Assert.Equal(1, firedCount);

			_dummyEventSource.Actuate(keys.Reverse());
			_dummyEventSource.Release(keys.Reverse());

			// Wrong order does not trigger
			Assert.Equal(1, firedCount);
		}
		[StaFact]
		public void Register_EnforceCombinationOrder_ModifierOrderAgnostic()
		{
			// It should be possible to actuate the modifier keys in any order
			// and still trigger the hotkey.

			int firedCount = 0;

			_registrar.Register(
				modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt,
				key:            Key.A,
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			// All these orders should see the hotkey triggered
			var validOrders = new []
			{
				/*
					* a b c
					* c a b
					* b c a
					* b a c
					* a c b
					* c b a
					*/

				new[] { Key.LeftCtrl,   Key.LeftShift,  Key.LeftAlt,    Key.A },
				new[] { Key.LeftAlt,    Key.LeftCtrl,   Key.LeftShift,  Key.A },
				new[] { Key.LeftShift,  Key.LeftAlt,    Key.LeftCtrl,   Key.A },
				new[] { Key.LeftShift,  Key.LeftCtrl,   Key.LeftAlt,    Key.A },
				new[] { Key.LeftCtrl,   Key.LeftAlt,    Key.LeftShift,  Key.A },
				new[] { Key.LeftAlt,    Key.LeftShift,  Key.LeftCtrl,   Key.A },
			};

			for (int i = 0; i < validOrders.Length; i++)
			{
				_dummyEventSource.Actuate(validOrders[i]);
				_dummyEventSource.Release(validOrders[i]);

				Assert.Equal(i + 1, firedCount);
			}
		}
		[StaFact]
		public void Register_EnforceCombinationOrder_RegularKeyLastOnly()
		{
			// The regular key must always come after the modifier keys in order
			// to trigger the hotkey, and so the hotkey shouldn't be triggered if
			// it's actuated in any order position.

			int firedCount = 0;

			_registrar.Register(
				modifierKeys:   ModifierKeys.Control | ModifierKeys.Shift,
				key:            Key.M,
				repeats:        false,
				firedHandler:   (s, e) => firedCount++
				);

			// Valid orders confirmed in another test
			var invalidOrders = new[]
			{
				new[] { Key.RightCtrl,  Key.M,          Key.RightShift },
				new[] { Key.RightShift, Key.M,          Key.RightCtrl  },
				new[] { Key.M,          Key.RightShift, Key.RightCtrl  },
				new[] { Key.M,          Key.RightCtrl,  Key.RightShift },
			};

			for (int i = 0; i < invalidOrders.Length; i++)
			{
				_dummyEventSource.Actuate(invalidOrders[i]);
				_dummyEventSource.Release(invalidOrders[i]);

				// Invalid orders should never trigger the hotkey
				Assert.Equal(0, firedCount);
			}
		}
	}
}
