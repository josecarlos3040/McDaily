using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasTransition : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Screens Container")]
    public RectTransform screensContainer;

    [Header("Screen Positions")]
    public float[] screenPositions =
    {
        0f,
        1518f,
        3074f,
        4669f,
        6255f
    };

    [Header("Transition")]
    public float slideDuration = 0.25f;

    [Header("Swipe")]
    public float swipeThreshold = 100f;

    [Header("Current Screen")]
    public int currentIndex = 0;


    // =====================================================
    // MENU INFERIOR
    // =====================================================

    [Header("Menu Inferior")]

    // Bolinha amarela
    [SerializeField]
    private RectTransform selectedIndicator;

    // Image filho da bolinha
    [SerializeField]
    private Image selectedIndicatorIcon;

    // Posição de cada botão
    [SerializeField]
    private RectTransform[] menuButtons;

    // Image do ícone de cada botão
    [SerializeField]
    private Sprite[] menuIcons;


    // =====================================================
    // DRAG
    // =====================================================

    private bool dragging;
    private bool transitioning;

    private Vector2 dragStartPosition;

    private float containerStartX;

    private float currentDragDeltaX;


    private void Start()
    {
        SetScreenInstant(
            currentIndex
        );
    }


    // =====================================================
    // COMEÇOU A ARRASTAR
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        if (transitioning)
        {
            return;
        }

        if (screensContainer == null)
        {
            return;
        }


        RectTransform parent =
            screensContainer.parent
            as RectTransform;


        if (parent == null)
        {
            return;
        }


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out dragStartPosition
            );


        containerStartX =
            screensContainer
                .anchoredPosition.x;


        currentDragDeltaX = 0f;

        dragging = true;
    }


    // =====================================================
    // ARRASTANDO
    // =====================================================

    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (!dragging)
        {
            return;
        }


        RectTransform parent =
            screensContainer.parent
            as RectTransform;


        if (parent == null)
        {
            return;
        }


        Vector2 currentPosition;


        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out currentPosition
            );


        float deltaX =
            currentPosition.x -
            dragStartPosition.x;


        currentDragDeltaX =
            deltaX;


        float newX =
            containerStartX +
            deltaX;


        float maxX =
            -screenPositions[0];


        float minX =
            -screenPositions[
                screenPositions.Length - 1
            ];


        newX =
            Mathf.Clamp(
                newX,
                minX,
                maxX
            );


        screensContainer
            .anchoredPosition =
            new Vector2(
                newX,
                screensContainer
                    .anchoredPosition.y
            );
    }


    // =====================================================
    // SOLTOU
    // =====================================================

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        if (!dragging)
        {
            return;
        }


        dragging = false;


        FinishSwipe(
            currentDragDeltaX
        );
    }


    // =====================================================
    // FINAL DO SWIPE
    // =====================================================

    private void FinishSwipe(
        float deltaX
    )
    {
        // Arrastou para esquerda
        // Vai para próxima tela
        if (
            deltaX <
            -swipeThreshold
        )
        {
            if (
                currentIndex <
                screenPositions.Length - 1
            )
            {
                StartCoroutine(
                    MoveToScreen(
                        currentIndex + 1
                    )
                );

                return;
            }
        }


        // Arrastou para direita
        // Vai para tela anterior
        if (
            deltaX >
            swipeThreshold
        )
        {
            if (
                currentIndex > 0
            )
            {
                StartCoroutine(
                    MoveToScreen(
                        currentIndex - 1
                    )
                );

                return;
            }
        }


        // Não arrastou o suficiente
        // Volta para tela atual
        StartCoroutine(
            MoveToScreen(
                currentIndex
            )
        );
    }


    // =====================================================
    // BOTÕES DO MENU
    // =====================================================

    public void GoToScreen(
        int index
    )
    {
        if (
            index < 0 ||
            index >= screenPositions.Length
        )
        {
            return;
        }


        if (transitioning)
        {
            return;
        }


        dragging = false;


        StartCoroutine(
            MoveToScreen(
                index
            )
        );
    }


    // =====================================================
    // MOVER PARA TELA
    // =====================================================

    private IEnumerator MoveToScreen(
        int index
    )
    {
        if (
            index < 0 ||
            index >= screenPositions.Length
        )
        {
            yield break;
        }


        transitioning = true;


        // Já atualiza a bolinha do menu
        UpdateMenuIndicator(
            index
        );


        float startX =
            screensContainer
                .anchoredPosition.x;


        float targetX =
            -screenPositions[index];


        float elapsed = 0f;


        while (
            elapsed <
            slideDuration
        )
        {
            elapsed +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed /
                    slideDuration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            float x =
                Mathf.Lerp(
                    startX,
                    targetX,
                    t
                );


            screensContainer
                .anchoredPosition =
                new Vector2(
                    x,
                    screensContainer
                        .anchoredPosition.y
                );


            yield return null;
        }


        screensContainer
            .anchoredPosition =
            new Vector2(
                targetX,
                screensContainer
                    .anchoredPosition.y
            );


        currentIndex =
            index;


        transitioning =
            false;
    }


    // =====================================================
    // POSIÇÃO INICIAL
    // =====================================================

    private void SetScreenInstant(
        int index
    )
    {
        index =
            Mathf.Clamp(
                index,
                0,
                screenPositions.Length - 1
            );


        currentIndex =
            index;


        screensContainer
            .anchoredPosition =
            new Vector2(
                -screenPositions[index],
                screensContainer
                    .anchoredPosition.y
            );


        UpdateMenuIndicator(
            index
        );
    }


    // =====================================================
    // ATUALIZAR BOLINHA DO MENU
    // =====================================================

    private void UpdateMenuIndicator(
        int index
    )
    {
        if (selectedIndicator == null)
        {
            return;
        }


        if (
            menuButtons == null ||
            index >= menuButtons.Length
        )
        {
            return;
        }


        if (
            menuButtons[index] == null
        )
        {
            return;
        }


        // Move a bolinha para cima do botão
        selectedIndicator.position =
            menuButtons[index].position;


        // Troca o ícone dentro da bolinha
        if (
            selectedIndicatorIcon != null &&
            menuIcons != null &&
            index < menuIcons.Length &&
            menuIcons[index] != null
        )
        {
            selectedIndicatorIcon.sprite =
                menuIcons[index];
        }
    }
}