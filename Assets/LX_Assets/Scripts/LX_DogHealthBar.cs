using UnityEngine;
using UnityEngine.UI;

namespace LX_Game
{
    /// <summary>
    /// 狗的血条UI管理器
    /// 显示狗的当前血量
    /// </summary>
    public class LX_DogHealthBar : MonoBehaviour
    {
        [Header("UI组件")]
        public Slider healthSlider; // 血条滑动条
        
        public Image fillImage; // 血条填充图片

        [Header("颜色设置")]
        public Color fullHealthColor = Color.green; // 满血颜色
        public Color lowHealthColor = Color.red; // 低血颜色
        public float lowHealthThreshold = 0.3f; // 低血阈值（30%）

        [Header("动画")]
        public bool useSmoothing = true; // 是否平滑过渡
        public float smoothSpeed = 5f; // 平滑速度

        private float targetValue = 1f;
        private DogAIController dog;

        void Start()
        {
            // 自动查找狗对象
            if (dog == null)
            {
                dog = FindObjectOfType<DogAIController>();
            }

            // 初始化UI
            if (dog != null)
            {
                UpdateHealth(dog.GetCurrentHealth(), dog.GetMaxHealth());
            }
            else
            {
                Debug.LogWarning("LX_DogHealthBar: 未找到DogAIController！");
            }
        }

        void Update()
        {
            // 平滑过渡血条
            if (useSmoothing && healthSlider != null)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, smoothSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 更新血量显示
        /// </summary>
        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (healthSlider == null) return;

            // 计算血量百分比
            float healthPercent = (float)currentHealth / maxHealth;
            targetValue = healthPercent;

            // 如果不使用平滑，直接设置
            if (!useSmoothing)
            {
                healthSlider.value = targetValue;
            }

            // 更新颜色
            UpdateColor(healthPercent);

            Debug.Log($"血条更新：{currentHealth}/{maxHealth} ({healthPercent * 100}%)");
        }

        /// <summary>
        /// 根据血量百分比更新颜色
        /// </summary>
        void UpdateColor(float healthPercent)
        {
            if (fillImage == null) return;

            // 根据血量比例在满血色和低血色之间插值
            if (healthPercent > lowHealthThreshold)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                    (healthPercent - lowHealthThreshold) / (1f - lowHealthThreshold));
            }
            else
            {
                fillImage.color = lowHealthColor;
            }
        }

        /// <summary>
        /// 显示血条
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏血条
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 重置血条到满血
        /// </summary>
        public void ResetToFull()
        {
            if (dog != null)
            {
                UpdateHealth(dog.GetMaxHealth(), dog.GetMaxHealth());
            }
        }
    }
}

