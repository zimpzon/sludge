using Sludge.Utility;
using UnityEngine;

namespace Sludge.Shared
{
    public class LevelDataTransform
    {
        public double PosX;
        public double PosY;
        public double ScaleX = 0.5f;
        public double ScaleY = 0.5f;
        public double RotZ;
        public double? Width;
        public double? Height;

        public static LevelDataTransform Get(Transform transform)
        {
            var result = new LevelDataTransform
            {
                PosX = SludgeUtil.Stabilize(transform.localPosition.x),
                PosY = SludgeUtil.Stabilize(transform.localPosition.y),
                ScaleX = SludgeUtil.Stabilize(transform.localScale.x),
                ScaleY = SludgeUtil.Stabilize(transform.localScale.y),
                RotZ = SludgeUtil.Stabilize(transform.rotation.eulerAngles.z),
            };

            var rectTrans = transform.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                result.Width = rectTrans.sizeDelta.x;
                result.Height = rectTrans.sizeDelta.y;
            }
            return result;
        }

        public void Set(Transform transform)
        {
            transform.localPosition = new Vector3((float)PosX, (float)PosY, 0);
            transform.localScale = new Vector3((float)ScaleX, (float)ScaleY, 1);
            transform.rotation = Quaternion.Euler(0, 0, (float)RotZ);
            if (Width != null)
            {
                var rectTrans = transform.GetComponent<RectTransform>();
                rectTrans.sizeDelta = new Vector2((float)Width, (float)Height);
            }
        }
    }
}
