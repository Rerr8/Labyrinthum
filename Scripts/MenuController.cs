using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    //Buttons
    public Button playButton;
    public Button exitGameButton;
    public Button kruskalButton;
    public Button backtrackingButton;
    public Button primButton;
    public Button ellerButton;
    public Button wilsonButton;
    public Button aldousBroderButton;
    public Button huntAndKillButton;
    public Button binaryTreeButton;
    public Button recursiveDivisionButton;
    public Button sidewinderButton;


    //Images
    public Image mainMenuBackGroundImage;
    public Image algBackGroundImage;

    //Scenes
    public string mazeSceneName = "SampleScene";

    void Start()
    {
        //Buttons
        playButton.gameObject.SetActive(true);
        exitGameButton.gameObject.SetActive(true);
        kruskalButton.gameObject.SetActive(false);
        backtrackingButton.gameObject.SetActive(false);
        primButton.gameObject.SetActive(false);
        ellerButton.gameObject.SetActive(false);
        wilsonButton.gameObject.SetActive(false);
        aldousBroderButton.gameObject.SetActive(false);
        huntAndKillButton.gameObject.SetActive(false);
        binaryTreeButton.gameObject.SetActive(false);
        recursiveDivisionButton.gameObject.SetActive(false);
        sidewinderButton.gameObject.SetActive(false);        


        //Images
        mainMenuBackGroundImage.gameObject.SetActive(true);
        algBackGroundImage.gameObject.SetActive(false);

        //Listeners
        playButton.onClick.AddListener(ShowAlgorithmMenu);
        exitGameButton.onClick.AddListener(ExitGame);
        kruskalButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Kruskal));
        backtrackingButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Backtracking));
        primButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Prim));
        ellerButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Eller));
        wilsonButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Wilson));
        aldousBroderButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.AldousBroder));
        huntAndKillButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.HuntAndKill));
        binaryTreeButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.BinaryTree));
        recursiveDivisionButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.RecursiveDivision));
        sidewinderButton.onClick.AddListener(() => SelectAndLoad(MazeSettings.Algorithm.Sidewinder));
    }

    private void ShowAlgorithmMenu()
    {
        //Images
        mainMenuBackGroundImage.gameObject.SetActive(false);
        algBackGroundImage.gameObject.SetActive(true);
        //Buttons
        playButton.gameObject.SetActive(false);
        exitGameButton.gameObject.SetActive(false);
        kruskalButton.gameObject.SetActive(true);
        backtrackingButton.gameObject.SetActive(true);
        primButton.gameObject.SetActive(true);
        ellerButton.gameObject.SetActive(true);
        wilsonButton.gameObject.SetActive(true);
        aldousBroderButton.gameObject.SetActive(true);
        huntAndKillButton.gameObject.SetActive(true);
        binaryTreeButton.gameObject.SetActive(true);
        recursiveDivisionButton.gameObject.SetActive(true);
        sidewinderButton.gameObject.SetActive(true); 
    }

    private void SelectAndLoad(MazeSettings.Algorithm alg)
    {
        MazeSettings.SelectedAlgorithm = alg;
        SceneManager.LoadScene(mazeSceneName);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}