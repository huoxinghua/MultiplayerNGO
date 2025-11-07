using System;
using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKInteractable : MonoBehaviour
    {
        [Header("Hands and Fingers Position")] 
        [SerializeField] private Transform handR;
        [SerializeField] private Transform handL;
        [SerializeField] private IkInteractSO ikInteractSo; 
        
        private Tween currentTween;
        private PlayerIKController _currentFPSIKController;
        private PlayerIKController _currentTPSIKController;
        public void PickupAnimation(PlayerIKController ikController, bool isFPS)
        {
            ikController.IKPos(this, handL, handR, ikInteractSo);
            ikController.IkActive = true;
            if (isFPS)
            {
                _currentFPSIKController = ikController;
            }
            else
            {
                _currentTPSIKController = ikController;  
            }
            transform.localPosition = ApplyStartPos(isFPS);
            transform.localRotation = Quaternion.Euler(ApplyStartRot(isFPS));
            PlayIKIdle(isFPS);
        }

        public void DropAnimation()
        {
            if (_currentFPSIKController != null)
            {
                _currentFPSIKController.IkActive = false;
                _currentFPSIKController.IKPos(null, null, null, null);
                _currentFPSIKController =  null;
            }
            if (_currentTPSIKController != null)
            {
                _currentTPSIKController.IkActive = false;
                _currentTPSIKController.IKPos(null, null, null, null);
                _currentTPSIKController =  null;
            }
            
            
        }

        public void PlayIKIdle(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyStartPos(isFPS)) duration = ikInteractSo.ikIdle.resetDuration;
            else duration = ikInteractSo.ikIdle.transitionDuration;

            transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikIdle, 0, isFPS), duration)
                        .SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikIdle, 1, isFPS), ikInteractSo.ikIdle.loopDuration)
                            .SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikIdle, 1, isFPS), ikInteractSo.ikIdle.loopDuration)
                            .SetEase(ikInteractSo.ikIdle.easeType))
                            .SetLoops(-1, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk(float slowSpeed,bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration * slowSpeed;
            
            if (transform.localPosition != ApplyStartPos(isFPS)) duration = ikInteractSo.ikWalk.resetDuration;
            else duration = ikInteractSo.ikWalk.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikIdle, 0, isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .Join(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikWalk, 0, isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyWaypoints(ikInteractSo.ikWalk, isFPS), ikInteractSo.ikWalk.loopDuration, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode)
                        .SetEase(ikInteractSo.ikWalk.easeType)
                        .SetLoops(-1, ikInteractSo.ikWalk.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                        rotateSeq.Append(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikWalk, 1, isFPS), ikInteractSo.ikWalk.loopDuration)
                                .SetEase(ikInteractSo.ikWalk.easeType))
                            .Append(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikWalk, 0, isFPS), ikInteractSo.ikWalk.loopDuration)
                                .SetEase(ikInteractSo.ikWalk.easeType))
                            .SetLoops(-1, ikInteractSo.ikWalk.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
        }

        public void PlayIKRun(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;
            
            if (transform.localPosition != ApplyStartPos(isFPS)) duration = ikInteractSo.ikRun.resetDuration;
            else duration = ikInteractSo.ikRun.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikIdle, 0, isFPS), duration)
                    .SetEase(ikInteractSo.ikRun.easeType))
                .Join(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikRun, 0, isFPS), duration)
                    .SetEase(ikInteractSo.ikRun.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyWaypoints(ikInteractSo.ikRun, isFPS), ikInteractSo.ikRun.loopDuration, ikInteractSo.ikRun.pathType, ikInteractSo.ikRun.pathMode)
                        .SetEase(ikInteractSo.ikRun.easeType)
                        .SetLoops(-1, ikInteractSo.ikRun.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                    rotateSeq.Append(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikRun, 1, isFPS), ikInteractSo.ikRun.loopDuration)
                            .SetEase(ikInteractSo.ikRun.easeType))
                        .Append(transform.DOLocalRotate(ApplyFollow(ikInteractSo.ikRun, 0, isFPS), ikInteractSo.ikRun.loopDuration)
                            .SetEase(ikInteractSo.ikRun.easeType))
                            .SetLoops(-1, ikInteractSo.ikRun.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
            
        }

        public void PlayIKInteract(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyStartPos(isFPS)) duration = ikInteractSo.ikInteract.resetDuration;
            else duration = ikInteractSo.ikInteract.transitionDuration;

            var seq = DOTween.Sequence();

            seq.Append(transform.DOLocalMove(ApplyStartPos(isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyStartRot(isFPS), duration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikInteract, 0, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti))
                .Join(transform.DOLocalRotate(ApplyRotpoints(ikInteractSo.ikInteract, 0, isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyWaypoints(ikInteractSo.ikInteract, 1, isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyRotpoints(ikInteractSo.ikInteract, 1, isFPS), ikInteractSo.ikInteract.hitDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            seq.Append(transform.DOLocalMove(ApplyStartPos(isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit))
                .Join(transform.DOLocalRotate(ApplyStartRot(isFPS), ikInteractSo.ikInteract.transitionDuration)
                    .SetEase(ikInteractSo.ikInteract.easeHit));

            currentTween = seq;

        }

        public void StopIKAnimation()
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
        }

        private Vector3 ApplyStartPos(bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            return Vector3.zero +  offset; 
        }
        
        private Vector3 ApplyStartRot(bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset;
            return Vector3.zero +  offset; 
        }

       private Vector3 ApplyWaypoints(IdlePreset preset, int index, bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            Vector3[] points = isFPS ? preset.fpsWaypoints : preset.tpsWaypoints;
            return points[index] + offset;
        }
        
        private Vector3[] ApplyWaypoints(MovementPreset preset, bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            Vector3[] points = isFPS ? preset.fpsWaypoints : preset.tpsWaypoints;

            Vector3[] result = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
        
        private Vector3 ApplyFollow(MovementPreset preset, int index, bool isFPS)
        {
            Vector3 offset = isFPS ? ikInteractSo.fpsOffset.rotOffset :  ikInteractSo.tpsOffset.rotOffset;
            Vector3[] points = isFPS ? preset.fpsFollowThrough : preset.tpsFollowThrough;
            return points[index] +  offset;
        }

        private Vector3 ApplyWaypoints(InteractPreset preset, int index, bool isFPS)
        {
            Vector3 posOffset = isFPS ? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            Vector3[] posPoints = isFPS ? preset.fpsPosWaypoints : preset.tpsPosWaypoints;
            
            return posPoints[index] + posOffset;
        }

        private Vector3 ApplyRotpoints(InteractPreset preset, int index, bool isFPS)
        {
            Vector3 rotOffset = isFPS ? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset;
            Vector3[] rotPoints = isFPS ? preset.fpsRotWaypoints :  preset.tpsRotWaypoints;
            
            return rotPoints[index] + rotOffset;
        }
    }
}

[Serializable]
public struct OffsetPos
{
    public Vector3 posOffset;
    public Vector3 rotOffset;
}

[Serializable]
public struct IdlePreset
{
    public float resetDuration;
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] fpsWaypoints;
    public Vector3[] tpsWaypoints;
    public LoopType loopType;
    public Ease easeType;
}

[Serializable]
public struct MovementPreset
{
    public float resetDuration;
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] fpsWaypoints;
    public Vector3[] tpsWaypoints;
    public Vector3[] fpsFollowThrough;
    public Vector3[] tpsFollowThrough;
    public LoopType loopType;
    public Ease easeType;
    public PathType pathType;
    public PathMode pathMode;
}

[Serializable]
public struct InteractPreset
{
    public float resetDuration;
    public float transitionDuration;
    public float hitDuration;
    public Vector3[] fpsPosWaypoints;
    public Vector3[] tpsPosWaypoints;
    public Vector3[] fpsRotWaypoints;
    public Vector3[] tpsRotWaypoints;
    public Ease easeAnti;
    public Ease easeHit;
}
