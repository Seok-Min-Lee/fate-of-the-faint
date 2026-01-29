//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class CardView : MonoBehaviour
//{
//    [SerializeField] private ViewType type;
//    [SerializeField] private TextMeshProUGUI cost;
//    [SerializeField] private TextMeshProUGUI name;
//    [SerializeField] private TextMeshProUGUI desc;
//    [SerializeField] private Image image;

//    public ViewType Type => type;

//    public CardInstance CardInstance { get; private set; }
//    public CardSystem CardSystem { get; private set; }
//    public CardViewPool Pool { get; private set; }
//    public void PlayCardStart()
//    {
//        CardSystem.PlayCardStart(this);
//    }
//    public void Init(CardInstance cardInstance, CardSystem cardSystem, CardViewPool pool)
//    {
//        CardInstance = cardInstance;
//        CardSystem = cardSystem;
//        Pool = pool;

//        cost.text = cardInstance.BaseDef.Cost.ToString();
//        name.text = cardInstance.BaseDef.Name;
//        desc.text = cardInstance.BaseDef.Description;
//        image.sprite = cardInstance.BaseDef.Image;

//        gameObject.SetActive(true);
//    }
//    public void PlayCardEnd()
//    {
//        Pool.Push(this);
//        transform.parent = Pool.transform;
//        gameObject.SetActive(false);
//    }
//    public enum ViewType
//    {
//        Attack,
//        Skill,
//        Power,
//    }
//}

