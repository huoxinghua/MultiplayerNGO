using UnityEngine;

[CreateAssetMenu(fileName = "FingerPoseSO", menuName = "FingerPoseSOs/FingerPoseSO")]
public class FingerPoseSO : ScriptableObject
{
    public FingerData thumbR;
    public FingerData indexR;
    public FingerData middleR;
    public FingerData ringR;
    public FingerData littleR;
    public FingerData thumbL;
    public FingerData indexL;
    public FingerData middleL;
    public FingerData ringL;
    public FingerData littleL;

    private void OnEnable()
    {
        if (thumbR.proximal.w == 0)
        {
            ResetAllFingerData();
        }
    }
    private void Reset()
    {
        ResetAllFingerData();
    }

    private void ResetAllFingerData()
    {
        thumbR = new FingerData(true);
        indexR = new FingerData(true);
        middleR = new FingerData(true);
        ringR = new FingerData(true);
        littleR = new FingerData(true);
        thumbL = new FingerData(true);
        indexL = new FingerData(true);
        middleL = new FingerData(true);
        ringL = new FingerData(true);
        littleL = new FingerData(true);
    }
}

[System.Serializable]
public struct FingerData
{
    public Quaternion proximal;
    public Quaternion intermediate;
    public Quaternion distal;

    public FingerData(bool initialize = true)
    {
        proximal = Quaternion.Euler(1, 1, 1);
        intermediate = Quaternion.Euler(1, 1, 1);
        distal = Quaternion.Euler(1, 1, 1);
    }
}
