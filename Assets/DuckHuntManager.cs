using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LQ;

public class DuckHuntManager : MonoBehaviour
{

    enum FlyDirection
    {
        left,
        right
    }

    enum DuckCategory { 
        blue,
        dark,
        nil
    }

    private float bottomPercent = 0.3f;
    private int level = 0;
    private int bulletNum = 3;
    private bool startTimer = false;

    private int count = 0;
    //是否逃离
    private bool flyAway = false;
    //限定鸭子的运动是否开启
    private bool RandomFly = false;
    private float JoyXmoveSpeed = 0f;
    private float JoyYmoveSpeed = 0f;

    private bool isHitDuck = false;
    private float _upperNum = 7;
    //标记当前生成了多少只鸭子
    private int DuckNumCount = 0;

    //UI
    public Text textTimer;
    public Button BtnFire;
    public GameObject ImageLevel;
    public GameObject UiPanel;
    public Text TextLevel;
    public Canvas Canvas;
    public Text TextScore;

    [SerializeField]
    private Camera mainCam;
    [SerializeField]
    private SimpleTouchController joystick;
    [SerializeField]
    private AudioSource audioSourceBg;
    [SerializeField]
    private AudioSource audioSourceGun;
    public DuckHuntResourceRef resourcesRef;
    [SerializeField]
    private GameObject bOne, bTwo, bThree;
    private GameObject BlueDuck, DarkDuck;

    [SerializeField]
    private GameObject DogLaughMovie;
    [SerializeField]
    private GameObject OpeningMovie;
    [SerializeField]
    private GameObject ImageDuckResult;
    private List<GameObject> DuckHitResults = new List<GameObject>();
    [SerializeField]
    private ImageEffect_MoblieBloom imageEffect_MoblieBloom;
    private GameObject chooseObj;
    private int HitDuckNum = 0;
    private GameObject target;
    [SerializeField]
    private GameObject ImageAim;
    private bool IsNextLevel = false;
    public GameObject ImageGameOver;
    public GameObject ImageEscape;
    public GameObject sc500;
    public GameObject sc1000;
    public GameObject CatchDuckMovie;
    public GameObject BottomCollider;
    public GameObject AudioSourceDFly;
    private int HitDuckScore;
    private bool IsFall = false;
    private DuckCategory hitDuckCategory = DuckCategory.nil;
    private float[] LevelNums = { 0.4f, 0.65f, 0.85f, 0.9f, 0.95f, 1.02f, 1.05f, 1.07f, 1.09f, 1.1f, 1.13f, 1.16f, 1.19f, 1.2f, 1.22f, 1.24f, 1.28f, 1.32f, 1.34f, 1.36f, 1.38f, 1.4f, 1.44f, 1.48f, 1.5f };
    private float DuckMoveSpeed = 0;
    [SerializeField]
    private GameObject PointLeft;
    [SerializeField]
    private GameObject PointRight;
    public GameObject Environment;
    
    public DuckHuntUtility duckHuntUtility;

    //飞行方向
    float vel_x = 0f, vel_y = 0f, vel_z = 0f;

    // Start is called before the first frame update
    void Start()
    {
        BlueDuck = resourcesRef.GameObjectRef[0];
        DarkDuck = resourcesRef.GameObjectRef[1];
        GetHitNumElement();
        hitBloomEffectInit();
        dogLaughMovieInit();
        BtnFire.onClick.AddListener(FireEvent);
        JoystickSensitive();
        DuckMoveSpeed = 0.4f;
        Vector3 bornPos = RandomBornPoint();
        InstantiateDuck(bornPos);
        moveNavigation();
        openMovieInit();
        WaitForPlayAudio(11, audioSourceBg, resourcesRef.AudioRef[0]);
        showLevel(level);
        HitDuckScore = 0;
        IsNextLevel = false;
    }

    private void Update()
    {
        if (startTimer)
        {
            Timer();
        }
        if (target) {
            Vector3 _screenPos = mainCam.WorldToScreenPoint(ImageAim.transform.position);
            raycastAimPoint(_screenPos);
        }
        aimPointController();
        TextScore.text = HitDuckScore + "";
        //debug hit duck
        if (Input.GetKey(KeyCode.Space)) {
            chooseObj = target;
            FireEvent();
        }
    }

    private void FixedUpdate()
    {
        if (RandomFly)
        {
            if (target)
            {
                randomMover(target);
            }
        }
        if (target)
        {
            if (IsFall)
            {
                target.GetComponent<Rigidbody>().AddForce(-Vector3.up * 99, UnityEngine.ForceMode.Acceleration);
            }
        }
    }

    private void borderCheck()
    {
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.transform.position);
        //-- x > width axis judge
        if (screenPos.x > duckHuntUtility.GetScreenWidthAndHeight().x)
        {
            vel_x = -vel_x;
            //--在x轴方向发生改变之后，镜像鸭子来改变飞行动画方向
            if (judgeDirection(vel_x) == FlyDirection.left)
            {
                target.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                target.GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        //--x < width axis judge
        if (screenPos.x < 0)
        {
            vel_x = -vel_x;
            if (judgeDirection(vel_x) == FlyDirection.left)
            {
                target.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                target.GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        //--limit top position(Screen.Height)
        if (screenPos.y > duckHuntUtility.GetScreenWidthAndHeight().y)
        {
            vel_y = -vel_y;
        }
        //--limit bottom position
        if (screenPos.y < duckHuntUtility.GetScreenWidthAndHeight().y * bottomPercent)
        {
            vel_y = -vel_y;
        }
    }

    private FlyDirection judgeDirection(float _axis)
    {
        FlyDirection direction = FlyDirection.left;
        if (_axis < 0)
        {
            direction = FlyDirection.left;
        }
        else
        {
            direction = FlyDirection.right;
        }
        return direction;
    }

    private void randomMover(GameObject _obj)
    {
        if (_obj)
        {
            _obj.transform.Translate(vel_x * DuckMoveSpeed, vel_y * DuckMoveSpeed, 0, Space.Self);
        }
        if (flyAway == false)
        {
            borderCheck();
        }
        else
        {
            if (target)
            {
                DuckFlyAway();
            }
        }
    }

    private void aimPointController()
    {
        if (joystick)
        {
            Vector3 localPos = ImageAim.transform.localPosition;
            float x = localPos.x + joystick.GetTouchPosition.x * JoyXmoveSpeed;
            float y = localPos.y + joystick.GetTouchPosition.y * JoyYmoveSpeed;
            Vector3 movePos = new Vector3(x, y, localPos.z);
            //Debug.Log(movePos);
            
            //--local position to world position order conver to screenposition 需要本地转换为世界才能正确转换为ScreenPOS
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(mainCam, Canvas.transform.TransformPoint(movePos));
            Debug.Log(screenPos);
            //--clamp x in 0 - width clamp y in 0 - height 对移动范围限定在屏幕范围内
            if (screenPos.x > 0 && screenPos.x < duckHuntUtility.GetScreenWidthAndHeight().x && screenPos.y > 0 && screenPos.y < duckHuntUtility.GetScreenWidthAndHeight().y)
            {
                ImageAim.transform.localPosition = movePos;
            }
        }
    }

    private void raycastAimPoint(Vector3 _screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(new Vector3(_screenPos.x, _screenPos.y, 0));
        RaycastHit hit = AimRaycast(ray, Mathf.Infinity);
        if (hit.collider)
        {
            chooseObj = hit.collider.gameObject;
        }
        else
        {
            chooseObj = null;
        }
    }

    private RaycastHit AimRaycast(Ray _ray, float rayDistance)
    {
        RaycastHit _hit;
        RaycastHit hitResult;
        if (Physics.Raycast(_ray, out _hit, rayDistance))
        {
            hitResult = _hit;
        }
        else
        {
            hitResult = new RaycastHit();
            hitResult.point = Vector3.zero;
        }
        return hitResult;
    }

    private void showLevel(int _level)
    {
        ImageLevel.gameObject.SetActive(true);
        ImageLevel.transform.GetChild(0).GetComponent<Text>().text = _level + "";
    }

    private void openMovieInit()
    {
        OpeningMovie.SetActive(true);
        OpeningMovie.GetComponent<DuckHuntUMovie>().OnAnimationPlayEnd.AddListener(() =>
        {
            startGame();
        });
    }

    //opening is end init game
    private void startGame()
    {
        ImageLevel.gameObject.SetActive(false);
        UiPanel.gameObject.SetActive(true);
        OpeningMovie.gameObject.SetActive(false);
        StartCoroutine(DelayDestroyOpeningAnimation());
        TextLevel.text = level + "";
        target.gameObject.SetActive(true);
        RandomFly = true;
        startTimer = true;
        //duck fly audio
        AudioSourceDFly.gameObject.SetActive(true);
        StartCoroutine(WaitForChangeAnimation(0.9f));
    }

    private IEnumerator WaitForChangeAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        target.GetComponent<Animator>().SetTrigger("change");
        bottomPercent = 0.35f;
    }

    private IEnumerator DelayDestroyOpeningAnimation()
    {
        yield return new WaitForSeconds(1);
        DestroyImmediate(OpeningMovie, true);
        OpeningMovie = null;
        Resources.UnloadUnusedAssets();
    }

    private void moveNavigation()
    {
        vel_x = 2.5f;
        vel_y = 2.2f;
    }

    //Game Start~实例化游戏对象
    private void InstantiateDuck(Vector3 _pos)
    {
        DuckNumCount = DuckNumCount + 1;
        GameObject cloneObj = Instantiate(PickDuck());
        cloneObj.transform.SetParent(Environment.transform);
        cloneObj.transform.localPosition = _pos;
        target = cloneObj;
    }

    private GameObject PickDuck()
    {
        float ranNum = Random.Range(0, 100);
        if (ranNum <= 50)
        {
            return BlueDuck;
        }
        else
        {
            return DarkDuck;
        }
    }

    private Vector3 RandomBornPoint()
    {
        Vector3 leftPointPos = PointLeft.transform.localPosition;
        Vector3 rightPointPos = PointRight.transform.localPosition;
        float ranX = Random.Range(leftPointPos.x, rightPointPos.x);
        return new Vector3(ranX, leftPointPos.y, leftPointPos.z);
    }

    private void JoystickSensitive()
    {
        switch (duckHuntUtility.GetRunningPlatform())
        {
            case RunningPlatform.Android:
                JoyXmoveSpeed = 16;
                JoyYmoveSpeed = 15;
                break;
            case RunningPlatform.Ios:
                JoyXmoveSpeed = 22;
                JoyYmoveSpeed = 21;
                break;
            case RunningPlatform.UnityEditor:
                JoyXmoveSpeed = 16;
                JoyYmoveSpeed = 15;
                break;
        }

    }

    private void dogLaughMovieInit()
    {
        DogLaughMovie.GetComponent<DuckHuntUMovie>().OnAnimationPlayEnd.AddListener(dogLaughMoviePlayAction);
    }

    private void dogLaughMoviePlayAction()
    {
        DogLaughMovie.gameObject.SetActive(false);
    }

    private void FireEvent()
    {
        imageEffect_MoblieBloom.threshold = 0.335f;
        StartCoroutine(DisableBloom(0.2f));
        if (chooseObj)
        {
            isHitDuck = true;
            HitDuckNum += 1;
            target.GetComponent<Animator>().SetTrigger("toEmpty");
            HitNumManager();
            startTimer = false;
            BtnFire.GetComponent<Image>().raycastTarget = false;
            target.GetComponent<DuckHuntCollision>().EventEnter += CheckDuckFailDownEvent;
            string hitDuckName = target.name;
            DuckScoreManager(hitDuckName);
            BottomCollider.SetActive(true);
            RandomFly = true;
            StartCoroutine(DelayChangeFallAni());
            playAudio(audioSourceGun, resourcesRef.AudioRef[2]);
            AudioSourceDFly.SetActive(false);
        }
        else
        {
            playAudio(audioSourceGun, resourcesRef.AudioRef[1]);
            isHitDuck = false;
        }
        bulletNum -= 1;
        bulletNumManager(bulletNum);
    }

    private IEnumerator DelayChangeFallAni()
    {
        yield return new WaitForSeconds(0.5f);
        target.GetComponent<Animator>().SetTrigger("duckDie");
        ScoreFlag();
        IsFall = true;
    }

    private void ScoreFlag()
    {
        if (hitDuckCategory == DuckCategory.dark)
        {
            sc500.transform.localPosition = target.transform.localPosition;
            sc500.gameObject.SetActive(true);
        }
        else if (hitDuckCategory==DuckCategory.blue)
        {
            sc1000.transform.localPosition = target.transform.localPosition;
            sc1000.gameObject.SetActive(true);
        }
    }

    private void bulletNumManager(int num)
    {
        switch (num)
        {
            case 0:
                bOne.SetActive(false);
                bTwo.SetActive(false);
                bThree.SetActive(false);
                BtnFire.GetComponent<Image>().raycastTarget = false;
                //子弹打完也没打中 逃逸成功
                if (isHitDuck == false)
                {
                    flyAway = true;
                    DuckFlyAway();
                }
                break;
            case 1:
                bOne.SetActive(true);
                bTwo.SetActive(false);
                bThree.SetActive(false);
                break;
            case 2:
                bOne.SetActive(true);
                bTwo.SetActive(true);
                bThree.SetActive(false);
                break;
            case 3:
                bOne.SetActive(true);
                bTwo.SetActive(true);
                bThree.SetActive(true);
                break;
            default: break;
        }
    }

    /// <summary>
    /// 逃离
    /// </summary>
    private void DuckFlyAway()
    {
        count += 1;
        if (count > 1)
        {
            return;
        }
        HitNumManager();
        if (count != 10)
        {
            ImageEscape.SetActive(true);
        }
        startTimer = false;
        BtnFire.GetComponent<Image>().raycastTarget = false;
        DogLaughMovie.SetActive(true);
        //在关卡结束的时候会产生音频冲突，不播放逃逸的音效
        if (DuckNumCount != 10)
        {
            playAudio(audioSourceBg, resourcesRef.AudioRef[3]);
        }
        AudioSourceDFly.gameObject.SetActive(false);
        BottomCollider.gameObject.SetActive(false);
        if (target)
        {
            StartCoroutine(DuckFlyAwayObject());
        }
    }

    private IEnumerator ReBorn(float time)
    {
        yield return new WaitForSeconds(time);
        //还原背景颜色
        mainCam.backgroundColor = new Color(0.47f, 0.67f, 0.97f);
        ImageEscape.gameObject.SetActive(false);
        ImageLevel.gameObject.SetActive(false);
        Vector3 bornPos = RandomBornPoint();
        InstantiateDuck(bornPos);
        //duck fly background
        AudioSourceDFly.gameObject.SetActive(true);
        RandomFly = true;
        moveNavigation();
        //Time config
        _upperNum = 7;
        textTimer.color = Color.white;
        startTimer = true;
        flyAway = false;
        count = 0;
        //因为最开始生成的位置偏下，调整最底部的比例值防止触发边界事件
        bottomPercent = 0.3f;
        bulletNum = 3;
        bulletNumManager(bulletNum);
        WaitForChangeAnimation(0.9f);
        //激活射击按钮
        BtnFire.GetComponent<Image>().raycastTarget = true;
    }

    private IEnumerator DuckFlyAwayObject()
    {
        yield return new WaitForSeconds(2);
        RandomFly = false;
        Destroy(target);
        target = null;
        if (IsNextLevel != true && DuckNumCount != 10)
        {
            StartCoroutine(ReBorn(2));
        }
    }

    private void DuckScoreManager(string name)
    {
        if (name.Contains("DarkDuck"))
        {
            HitDuckScore += 500;
            hitDuckCategory = DuckCategory.dark;
        }
        else
        {
            HitDuckScore += 1000;
            hitDuckCategory = DuckCategory.blue;
        }
    }

    private void CheckDuckFailDownEvent(Collision _col)
    {
        if (target)
        {
            target.GetComponent<DuckHuntCollision>().EventEnter -= CheckDuckFailDownEvent;
            sc1000.gameObject.SetActive(false);
            sc500.gameObject.SetActive(false);
            IsFall = false;
            if (DuckNumCount != 10)
            {
                playAudio(audioSourceBg, resourcesRef.AudioRef[5]);
            }
            Destroy(target);
            target = null;
            CatchDuckMovie.gameObject.SetActive(true);
            isHitDuck = false;
            if (isHitDuck == false && DuckNumCount != 10)
            {
                StartCoroutine(ReBorn(2));
            }
        }
    }

    private void HitNumManager()
    {
        if (DuckNumCount == 10)
        {
            IsNextLevel = true;
            ImageAim.gameObject.SetActive(false);
            if (GameOver() == false)
            {
                HitDuckNum = 0;
                level += 1;
                StartCoroutine(NextLevelInit(4));
            }
        }

        if (flyAway)
        {
            DuckHitResults[DuckNumCount - 1].GetComponent<Image>().color = Color.gray;
        }

        if (isHitDuck)
        {
            DuckHitResults[DuckNumCount - 1].GetComponent<Image>().color = Color.red;
        }
    }

    private void Timer()
    {
        _upperNum = _upperNum - Time.deltaTime;
        textTimer.text = _upperNum + "";
        if (Mathf.Abs(_upperNum) < 2)
        {
            //Time run out reminder
            mainCam.backgroundColor = new Color(0.96f, 0.82f, 0.76f);
            textTimer.color = Color.red;
        }
        // is over
        if (Mathf.Abs(_upperNum) < 0.1)
        {
            startTimer = false;
            textTimer.text = "0.00000";
            flyAway = true;
        }
    }

    private IEnumerator NextLevelInit(float time)
    {
        yield return new WaitForSeconds(time);
        ImageEscape.gameObject.SetActive(false);
        ImageAim.gameObject.SetActive(true);
        LevelNumsManager();
        IsNextLevel = false;
        DuckNumCount = 0;
        ReBorn(2);
        showLevel(level);
        TextLevel.text = level + "";
        for (int i = 0; i < 9; i++)
        {
            DuckHitResults[i].GetComponent<Image>().color = Color.white;
        }
    }

    private void LevelNumsManager()
    {
        if (level % 10 == 0)
        {
            int index = level / 10;
            if (index <= 25)
            {
                DuckMoveSpeed = LevelNums[index];
            }
            else
            {
                DuckMoveSpeed = 1.5f;
            }
        }
    }

    private bool GameOver()
    {
        bool result = false;
        if (HitDuckNum < 6)
        {
            result = true;
            ImageGameOver.gameObject.SetActive(true);
            playAudio(audioSourceBg, resourcesRef.AudioRef[6]);
            StartCoroutine(gameOverUiInit());
        }
        else
        {
            result = false;
        }

        return result;
    }

    private IEnumerator gameOverUiInit()
    {
        yield return new WaitForSeconds(6);
        level = 1;
        HitDuckNum = 0;
        HitDuckScore = 0;
        ImageAim.gameObject.SetActive(false);
        ImageGameOver.gameObject.SetActive(false);
        ImageEscape.gameObject.SetActive(false);
        StartCoroutine(NextLevelInit(2));
    }

    private void hitBloomEffectInit()
    {
        imageEffect_MoblieBloom.threshold = 0;
        imageEffect_MoblieBloom.intensity = 1.01f;
    }

    private IEnumerator DisableBloom(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        imageEffect_MoblieBloom.threshold = 0f;
    }

    private IEnumerator WaitForPlayAudio(float time, AudioSource audioSource, AudioClip clip)
    {
        yield return new WaitForSeconds(time);
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void playAudio(AudioSource audioSource, AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// 获取击中鸭子的UI展示元素
    /// </summary>
    private void GetHitNumElement()
    {
        for (int i = 0; i < ImageDuckResult.transform.childCount; i++)
        {
            DuckHitResults.Add(ImageDuckResult.transform.GetChild(i).gameObject);
        }
    }

}
