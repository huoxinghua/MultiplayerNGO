using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using UnityEngine;
using DG.Tweening;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public abstract class IKItemAnimation : MonoBehaviour
    {
        /*[Header("Position/Rotation Anchor")]
        [SerializeField] private Transform gripAnchor;*/
        
        [field: SerializeField] public IkInteractSO ikInteractSo { get; private set; }
        private Tween transitionTween;
        private Tween interactTween;
        private float localAnimTime = 0f;
        public bool IsInteractComplete { get; private set; } = true;
        public Tween currentTween { get; set; }
        
        public virtual void PlayIKIdle(bool isFPS)
        {
            localAnimTime = 0f;

            var waypoints = isFPS ? ikInteractSo.ikIdle.fpsWaypoints :  ikInteractSo.ikIdle.tpsWaypoints;

            float distanceToTarget = Vector3.Distance(transform.localPosition, ApplyPosOffset(waypoints[0], isFPS));
                
            var duration = distanceToTarget/ikInteractSo.ikIdle.transitionDuration;

            transitionTween = transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    StopIKAnimation();
                    
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public virtual void PlayIKMove(float slowSpeed, bool isFPS, bool isRunning)
        {
            localAnimTime = 0f;

            var preset = isRunning ? ikInteractSo.ikRun : ikInteractSo.ikWalk;
            var waypoints = isFPS ? preset.fpsWaypoints : preset.tpsWaypoints;
            var followThroughs = isFPS ? preset.fpsFollowThrough : preset.tpsFollowThrough;
            
         
            float distanceToTarget = Vector3.Distance(transform.localPosition, ApplyPosOffset(waypoints[0], isFPS));
                
            var duration = distanceToTarget/preset.transitionDuration;


            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .Join(transform.DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyPosOffset(waypoints, isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode)
                        .SetEase(ikInteractSo.ikWalk.easeType)
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                    rotateSeq.Append(transform.DOLocalRotate(ApplyRotOffset(followThroughs[1], isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .Append(transform.DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
        }

        public virtual void PlayIKInteract(bool isFPS)
        {
            localAnimTime = 0f;
            IsInteractComplete = false;

            var waypoints = isFPS ? ikInteractSo.ikInteract.fpsPosWaypoints :  ikInteractSo.ikInteract.tpsPosWaypoints;
            var RotPoints = isFPS ? ikInteractSo.ikInteract.fpsRotWaypoints : ikInteractSo.ikInteract.tpsRotWaypoints;

            if (waypoints == null || waypoints.Length < 2 || RotPoints == null || RotPoints.Length < 2)
            {
                Debug.LogError($"[{gameObject.name}] Interact animation waypoints not configured in IkInteractSO!");
                IsInteractComplete = true;
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.localPosition, ApplyPosOffset(waypoints[0], isFPS));
                
            var duration = distanceToTarget/ikInteractSo.ikInteract.transitionDuration;

            var seq = DOTween.Sequence();

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.OnComplete(() =>
            {
                IsInteractComplete = true;
            });
            interactTween = seq;
        }
        
        public void StopIKAnimation()
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            
            if(transitionTween != null)
            {
                transitionTween.Kill(true);
                transitionTween = null;
            }
            
            if(interactTween != null)
            {
                interactTween.Kill(true);
                interactTween = null;
            }
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