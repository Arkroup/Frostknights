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
    public class StatusEffectBones : StatusEffectData
    {
        [CompilerGenerated]
        private sealed class _003CCheck2_003Ed__2 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public Hit hit;

            public StatusEffectBones _003C_003E4__this;

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
            public _003CCheck2_003Ed__2(int _003C_003E1__state)
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
                        if ((bool)hit.attacker && hit.attacker.canBeHit)
                        {
                            _003Chit2_003E5__1 = new Hit(_003C_003E4__this.target, hit.attacker, _003C_003E4__this.count)
                            {
                                canRetaliate = false,
                                damageType = "spikes"
                            };
                            _003C_003E2__current = _003Chit2_003E5__1.Process();
                            _003C_003E1__state = 1;
                            return true;
                        }

                        break;
                    case 1:
                        _003C_003E1__state = -1;
                        _003Chit2_003E5__1 = null;
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
        private sealed class _003CCheck_003Ed__4 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int _003C_003E1__state;

            private object _003C_003E2__current;

            public Hit hit;

            public StatusEffectBones _003C_003E4__this;

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
            public _003CCheck_003Ed__4(int _003C_003E1__state)
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
                        _003C_003E4__this.count -= hit.damageBlocked;
                        _003C_003E4__this.count -= hit.damage;
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
            base.PostHit += Check;
            base.OnHit += Check2;
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (hit.target == target && hit.canRetaliate && hit.Offensive && hit.BasicHit)
            {
                return hit.attacker != target;
            }

            return false;
        }

        [IteratorStateMachine(typeof(_003CCheck2_003Ed__2))]
        public IEnumerator Check2(Hit hit)
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CCheck2_003Ed__2(0)
            {
                _003C_003E4__this = this,
                hit = hit
            };
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.target == target && hit.canRetaliate && hit.Offensive && hit.BasicHit)
            {
                return hit.target == target;
            }

            return false;
        }

        [IteratorStateMachine(typeof(_003CCheck_003Ed__4))]
        public IEnumerator Check(Hit hit)
        {
            //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
            return new _003CCheck_003Ed__4(0)
            {
                _003C_003E4__this = this,
                hit = hit
            };
        }
    }
}