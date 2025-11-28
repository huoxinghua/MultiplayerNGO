using _Project.Code.Art.AnimationScripts.IK;
using UnityEngine;
using DG.Tweening;

public class IKTranqGunAnimation : IKItemAnimation
{
    public override void PlayIKInteract(bool isFPS)
    {
            localAnimTime = 0f;
            IsInteractComplete = false;

            var waypoints = isFPS ? ikInteractSo.ikInteract.fpsPosWaypoints :  ikInteractSo.ikInteract.tpsPosWaypoints;
            var RotPoints = isFPS ? ikInteractSo.ikInteract.fpsRotWaypoints : ikInteractSo.ikInteract.tpsRotWaypoints;

            if (waypoints == null || waypoints.Length < 1 || RotPoints == null || RotPoints.Length < 1)
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

            seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[0], isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.moveDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.moveDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.OnComplete(() =>
            {
                IsInteractComplete = true;
            });
            currentTween = seq;
    }
}
