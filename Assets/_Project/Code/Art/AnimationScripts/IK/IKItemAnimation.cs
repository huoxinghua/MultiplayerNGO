using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using UnityEngine;
using DG.Tweening;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public abstract class IKItemAnimation : MonoBehaviour
    {
        [field: SerializeField] public IkInteractSO ikInteractSo { get; private set; }
        private Tween interactTween;
        private Tween currentTween;
        public bool isInteract { get; private set; } = false;

        public virtual void PlayIKIdleLocal(bool isFPS)
        {
            if (currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            var waypoints = isFPS ? ikInteractSo.ikIdle.fpsWaypoints : ikInteractSo.ikIdle.tpsWaypoints;
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS))
                duration = ikInteractSo.ikIdle.resetDuration;
            else duration = ikInteractSo.ikIdle.transitionDuration;

            transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    if (currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }

                    var seq = DOTween.Sequence();
                    seq.Append(transform
                            .DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikIdle.loopDuration)
                            .SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(transform
                            .DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikIdle.loopDuration)
                            .SetEase(ikInteractSo.ikIdle.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public virtual void PlayIKMoveLocal(float slowSpeed, bool isFPS, bool isRunning)
        {
            if (currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            var preset = isRunning ? ikInteractSo.ikRun : ikInteractSo.ikWalk;
            var waypoints = isFPS ? preset.fpsWaypoints : preset.tpsWaypoints;
            var followThroughs = isFPS ? preset.fpsFollowThrough : preset.tpsFollowThrough;

            float duration = preset.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = preset.resetDuration;
            else duration = preset.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .Join(transform.DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .OnComplete(() =>
                {
                    if (currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }

                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyPosOffset(waypoints, isFPS),
                            ikInteractSo.ikWalk.loopDuration * slowSpeed, ikInteractSo.ikWalk.pathType,
                            ikInteractSo.ikWalk.pathMode)
                        .SetEase(ikInteractSo.ikWalk.easeType)
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                    rotateSeq.Append(transform
                            .DOLocalRotate(ApplyRotOffset(followThroughs[1], isFPS),
                                ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .Append(transform
                            .DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS),
                                ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
        }

        public virtual void PlayIKInteractLocal(bool isFPS)
        {
            if (isInteract) return;
            isInteract = true;
            if (interactTween != null)
            {
                interactTween.Kill(true);
                interactTween = null;
            }

            var waypoints = isFPS ? ikInteractSo.ikInteract.fpsPosWaypoints : ikInteractSo.ikInteract.tpsPosWaypoints;
            var RotPoints = isFPS ? ikInteractSo.ikInteract.fpsRotWaypoints : ikInteractSo.ikInteract.tpsRotWaypoints;
            float duration = ikInteractSo.ikInteract.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS))
                duration = ikInteractSo.ikInteract.resetDuration;
            else duration = ikInteractSo.ikInteract.transitionDuration;

            var seq = DOTween.Sequence();

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform
                    .DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform
                    .DOLocalRotate(ApplyRotOffset(RotPoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.Append(transform
                    .DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform
                    .DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.OnComplete(() => { isInteract = false; });
            interactTween = seq;
        }

        public Vector3 ApplyPosOffset(Vector3 point, bool isFPS)
        {
            return point + (isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset);
        }

        public Vector3[] ApplyPosOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }

        public Vector3 ApplyRotOffset(Vector3 point, bool isFPS)
        {
            return point + (isFPS ? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset);
        }

        public Vector3[] ApplyRotOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
    }
}