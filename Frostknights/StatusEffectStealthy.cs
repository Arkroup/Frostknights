using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;
using static SfxSystem;


namespace Frostknights
{
    public class StatusEffectStealthy : StatusEffectData
    {
        [CompilerGenerated]
        private sealed class _003CActionPerformed_003Ed__6 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public PlayAction action;

            public StatusEffectStealthy _003C_003E4__this;

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            [DebuggerHidden]
            public _003CActionPerformed_003Ed__6(int _003C_003E1__state)
            {
                this._003C_003E1__state = _003C_003E1__state;
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                _003C_003E1__state = -2;
            }

            private bool MoveNext()
            {
                switch (_003C_003E1__state)
                {
                    default:
                        return false;
                    case 0:
                        _003C_003E1__state = -1;
                        _003C_003E2__current = _003C_003E4__this.CountDown();
                        _003C_003E1__state = 1;
                        return true;
                    case 1:
                        _003C_003E1__state = -1;
                        return false;
                }
            }

            bool IEnumerator.MoveNext()
            {
                //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                return this.MoveNext();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }
        }

        [CompilerGenerated]
        private sealed class _003CCountDown_003Ed__7 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public StatusEffectStealthy _003C_003E4__this;

            private int _003Camount_003E5__1;

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return _003C_003E2__current;
                }
            }

            [DebuggerHidden]
            public _003CCountDown_003Ed__7(int _003C_003E1__state)
            {
                this._003C_003E1__state = _003C_003E1__state;
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                _003C_003E1__state = -2;
            }

            private bool MoveNext()
            {
                switch (_003C_003E1__state)
                {
                    default:
                        return false;
                    case 0:
                        _003C_003E1__state = -1;
                        _003Camount_003E5__1 = 1;
                        Events.InvokeStatusEffectCountDown(_003C_003E4__this, ref _003Camount_003E5__1);
                        _003C_003E4__this.cardPlayed = false;
                        if (_003Camount_003E5__1 != 0)
                        {
                            _003C_003E2__current = _003C_003E4__this.CountDown(_003C_003E4__this.target, _003Camount_003E5__1);
                            _003C_003E1__state = 1;
                            return true;
                        }

                        break;
                    case 1:
                        _003C_003E1__state = -1;
                        break;
                }

                return false;
            }

            bool IEnumerator.MoveNext()
            {
                //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                return this.MoveNext();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }
        }

        public bool cardPlayed;

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
        }

        public override bool RunBeginEvent()
        {
            target.cannotBeHitCount++;
            return false;
        }

        public override bool RunEndEvent()
        {
            target.cannotBeHitCount--;
            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0)
            {
                cardPlayed = true;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        [IteratorStateMachine(typeof(_003CActionPerformed_003Ed__6))]
        public IEnumerator ActionPerformed(PlayAction action)
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CActionPerformed_003Ed__6(0)
            {
                _003C_003E4__this = this,
                action = action
            };
        }

        [IteratorStateMachine(typeof(_003CCountDown_003Ed__7))]
        public IEnumerator CountDown()
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CCountDown_003Ed__7(0)
            {
                _003C_003E4__this = this
            };
        }
    }
}