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
    public class StatusEffectFracture : StatusEffectData
    {
        [CompilerGenerated]
        private sealed class _003CDealDamage2_003Ed__3 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public Entity entity;

            public StatusEffectFracture _003C_003E4__this;

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
            public _003CDealDamage2_003Ed__3(int _003C_003E1__state)
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
                        if (_003Camount_003E5__1 != 0)
                        {
                            _003C_003E2__current = _003C_003E4__this.CountDown(entity, _003Camount_003E5__1);
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

        [CompilerGenerated]
        private sealed class _003CHit_003Ed__2 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public Hit hit;

            public StatusEffectFracture _003C_003E4__this;

            private Hit _003Chit2_003E5__1;

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
            public _003CHit_003Ed__2(int _003C_003E1__state)
            {
                this._003C_003E1__state = _003C_003E1__state;
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                _003Chit2_003E5__1 = null;
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
                        hit.damage += _003C_003E4__this.count;
                        if (_003C_003E4__this.count <= 0)
                        {
                            _003C_003E2__current = _003C_003E4__this.Remove();
                            _003C_003E1__state = 1;
                            return true;
                        }

                        break;
                    case 1:
                        _003C_003E1__state = -1;
                        break;
                }

                _003Chit2_003E5__1 = new Hit(_003C_003E4__this.target, hit.attacker, _003C_003E4__this.count)
                {
                    canRetaliate = false,
                    damageType = "fracture"
                };
                _003C_003E4__this.target.PromptUpdate();
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

        public override void Init()
        {
            base.OnHit += Hit;
            base.OnTurnEnd += DealDamage2;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.Offensive && count > 0)
            {
                return hit.target == target;
            }

            return false;
        }

        [IteratorStateMachine(typeof(_003CHit_003Ed__2))]
        public IEnumerator Hit(Hit hit)
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CHit_003Ed__2(0)
            {
                _003C_003E4__this = this,
                hit = hit
            };
        }

        [IteratorStateMachine(typeof(_003CDealDamage2_003Ed__3))]
        public IEnumerator DealDamage2(Entity entity)
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CDealDamage2_003Ed__3(0)
            {
                _003C_003E4__this = this,
                entity = entity
            };
        }
    }
}