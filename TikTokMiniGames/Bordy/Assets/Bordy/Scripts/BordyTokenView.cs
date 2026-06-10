using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Renders a sun or moon token with Q-style art and light motion.
    /// 用 Q 版美术与轻量动效渲染太阳 / 月亮棋子。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BordyTokenView : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rect;
        private int _kind = BordyPuzzleData.Empty;
        private float _phase;
        private Coroutine _motion;

        private void Awake() => EnsureReady();

        private void EnsureReady()
        {
            if (_image != null)
                return;

            _image = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
            _image.preserveAspect = true;
            _image.raycastTarget = false;
        }

        public void SetKind(int kind, bool animate)
        {
            EnsureReady();
            if (_kind == kind)
                return;

            _kind = kind;
            if (kind == BordyPuzzleData.Empty)
            {
                Hide(animate);
                return;
            }

            _image.sprite = kind == BordyPuzzleData.Sun ? BordyTokenSprites.Sun : BordyTokenSprites.Moon;
            _image.color = Color.white;
            _image.enabled = true;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            RestartMotion(animate ? BordyTokenMotion.PopIn : BordyTokenMotion.Idle);
        }

        public void ShowStatic(int kind)
        {
            EnsureReady();
            if (_motion != null)
            {
                StopCoroutine(_motion);
                _motion = null;
            }

            _kind = kind;
            _rect.anchoredPosition = Vector2.zero;
            if (kind == BordyPuzzleData.Empty)
            {
                _image.enabled = false;
                _rect.localScale = Vector3.one;
                _rect.localRotation = Quaternion.identity;
                return;
            }

            _image.sprite = kind == BordyPuzzleData.Sun ? BordyTokenSprites.Sun : BordyTokenSprites.Moon;
            _image.color = Color.white;
            _image.enabled = true;
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
            _phase = 0f;
            RestartMotion(BordyTokenMotion.Idle);
        }

        private void Hide(bool animate)
        {
            if (!animate || !_image.enabled)
            {
                _image.enabled = false;
                _rect.localScale = Vector3.one;
                return;
            }

            RestartMotion(BordyTokenMotion.PopOut);
        }

        private void RestartMotion(BordyTokenMotion motion)
        {
            if (_motion != null)
                StopCoroutine(_motion);
            _motion = StartCoroutine(RunMotion(motion));
        }

        private IEnumerator RunMotion(BordyTokenMotion motion)
        {
            if (motion == BordyTokenMotion.PopIn)
            {
                float t = 0f;
                while (t < 0.22f)
                {
                    t += Time.deltaTime;
                    float p = t / 0.22f;
                    float scale = Mathf.Lerp(0f, 1.12f, EaseOutBack(p));
                    _rect.localScale = Vector3.one * scale;
                    yield return null;
                }

                t = 0f;
                while (t < 0.12f)
                {
                    t += Time.deltaTime;
                    float p = t / 0.12f;
                    _rect.localScale = Vector3.one * Mathf.Lerp(1.12f, 1f, p);
                    yield return null;
                }

                motion = BordyTokenMotion.Idle;
            }
            else if (motion == BordyTokenMotion.PopOut)
            {
                float t = 0f;
                var start = _rect.localScale;
                while (t < 0.14f)
                {
                    t += Time.deltaTime;
                    float p = t / 0.14f;
                    _rect.localScale = Vector3.Lerp(start, Vector3.zero, p);
                    _rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, _kind == BordyPuzzleData.Sun ? 18f : -18f, p));
                    yield return null;
                }

                _image.enabled = false;
                _rect.localScale = Vector3.one;
                _rect.localRotation = Quaternion.identity;
                _motion = null;
                yield break;
            }

            while (true)
            {
                _phase += Time.deltaTime;
                if (_kind == BordyPuzzleData.Sun)
                {
                    float bob = Mathf.Sin(_phase * 3.6f) * 3f;
                    float tilt = Mathf.Sin(_phase * 2.4f) * 4f;
                    float pulse = 1f + Mathf.Sin(_phase * 4.8f) * 0.04f;
                    _rect.anchoredPosition = new Vector2(0f, bob);
                    _rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
                    _rect.localScale = Vector3.one * pulse;
                }
                else
                {
                    float bob = Mathf.Sin(_phase * 2.8f) * 4f;
                    float sway = Mathf.Sin(_phase * 2.1f) * 3f;
                    float pulse = 1f + Mathf.Sin(_phase * 3.5f) * 0.035f;
                    _rect.anchoredPosition = new Vector2(sway, bob);
                    _rect.localRotation = Quaternion.Euler(0f, 0f, sway * 0.35f);
                    _rect.localScale = Vector3.one * pulse;
                }

                yield return null;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private enum BordyTokenMotion
        {
            Idle,
            PopIn,
            PopOut
        }
    }
}
