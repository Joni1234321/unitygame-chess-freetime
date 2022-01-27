using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour
{
    public Camera mainCamera;
    public Image turnVisualizer;
    public GameObject endOfGameVisual, promoteVisual, checkVisual;
    public Text winnerText;

    Chess chessGame;



    void CenterCameraOnBoard (SquareBoard board, int edge)
    {
        float nHalf = board.n * .5f;
        mainCamera.orthographicSize = nHalf + edge;
        mainCamera.transform.position = new Vector3(nHalf, 10, nHalf);
    }

    void ShowTurn ()
    {
        turnVisualizer.color = ChessPiece.PlayerColors[chessGame.playerTurn];
        checkVisual.SetActive(chessGame.board.checkedPlayer == chessGame.playerTurn);
    }
    void ShowGameOverScreen ()
    {
        endOfGameVisual.SetActive(true);
        winnerText.text = (chessGame.board.winner == 0 ? "WHITE" : "BLACK") + " WON!";
        Debug.Log("Game Over");
    }

    void ShowPromotePawnScreen ()
    {
        promoteVisual.SetActive(true);
    }

    public void PromotePawnTo(int promotion) => PromotePawnTo((ChessPiece.Type)promotion);
    public void PromotePawnTo(ChessPiece.Type promotion)
    {
        if (chessGame.TryPromotePawnTo(chessGame.promotingPiece, promotion))
            promoteVisual.SetActive(false);
    }

    public void RestartGame ()
    {
        chessGame.Restart();
        endOfGameVisual.SetActive(false);
        promoteVisual.SetActive(false);
        Debug.Log("Restarting Game");
        ShowTurn();

    }

    void Update()
    {
        if (chessGame.isRunning) {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
                chessGame.LeftClickDown(worldPos);
            else if (Input.GetMouseButtonUp(0))
                chessGame.LeftClickUp(worldPos);
            else if (Input.GetMouseButton(0))
                chessGame.LeftClick(worldPos);
            else if (Input.GetMouseButtonDown(1))
                chessGame.RightClickDown(worldPos);
        }

        if (Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

    void Start()
    {
        chessGame = new Chess();
        CenterCameraOnBoard(chessGame.board, 1);

        ShowTurn();
    }

    public static GameObject ShadowPrefab;
    public GameObject shadowPrefab;
    public static Material BoardMaterial;
    public Material boardMaterial;
    void Awake()
    {
        mainCamera = Camera.main;
        BoardMaterial = boardMaterial;
        ShadowPrefab = shadowPrefab;
    }

    void OnEnable()
    {
        Chess.OnNextTurn += ShowTurn;
        Chess.OnGameOver += ShowGameOverScreen;
        Chess.OnPawnPromotion += ShowPromotePawnScreen;
    }
    void OnDisable()
    {
        Chess.OnNextTurn -= ShowTurn;
        Chess.OnGameOver -= ShowGameOverScreen;
        Chess.OnPawnPromotion -= ShowPromotePawnScreen;

    }

}
