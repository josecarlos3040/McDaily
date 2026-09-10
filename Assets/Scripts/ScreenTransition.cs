using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CanvasTransition : MonoBehaviour
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

    private bool dragging;
    private bool transitioning;

    private Vector2 touchStart;
    private float containerStartX;

    private void Start()
    {
        SetScreenInstant(currentIndex);
    }

    private void Update()
    {
        HandleTouch();
    }

    // =====================================================
    // TOUCH / SWIPE
    // =====================================================

    private void HandleTouch()
    {
        if (Touchscreen.current == null)
            return;

        if (EventSystem.current == null)
            return;

        var touch = Touchscreen.current.primaryTouch;

        // =================================================
        // COMEÇOU O TOQUE
        // =================================================

        if (touch.press.wasPressedThisFrame)
        {
            // Não inicia swipe durante uma animação
            if (transitioning)
                return;

            // IMPORTANTE:
            // Se tocou em um botão ou qualquer outro elemento
            // da UI, NÃO trata isso como swipe.
            int touchId =
                touch.touchId.ReadValue();

            if (EventSystem.current.IsPointerOverGameObject(touchId))
            {
                dragging = false;
                return;
            }

            touchStart =
                touch.position.ReadValue();

            containerStartX =
                screensContainer.anchoredPosition.x;

            dragging = true;

            return;
        }

        // =================================================
        // NÃO ESTÁ ARRASTANDO
        // =================================================

        if (!dragging)
            return;

        // =================================================
        // MOVIMENTO
        // =================================================

        Vector2 currentPosition =
            touch.position.ReadValue();

        float deltaX =
            currentPosition.x - touchStart.x;

        float newX =
            containerStartX + deltaX;

        // Limite da primeira tela
        float maxX =
            -screenPositions[0];

        // Limite da última tela
        float minX =
            -screenPositions[screenPositions.Length - 1];

        newX =
            Mathf.Clamp(
                newX,
                minX,
                maxX
            );

        screensContainer.anchoredPosition =
            new Vector2(
                newX,
                screensContainer.anchoredPosition.y
            );

        // =================================================
        // SOLTOU O TOQUE
        // =================================================

        if (touch.press.wasReleasedThisFrame)
        {
            dragging = false;

            FinishSwipe(deltaX);
        }
    }

    // =====================================================
    // FINALIZA SWIPE
    // =====================================================

    private void FinishSwipe(float deltaX)
    {
        // Swipe para ESQUERDA
        if (deltaX < -swipeThreshold)
        {
            if (currentIndex < screenPositions.Length - 1)
            {
                StartCoroutine(
                    MoveToScreen(currentIndex + 1)
                );

                return;
            }
        }

        // Swipe para DIREITA
        if (deltaX > swipeThreshold)
        {
            if (currentIndex > 0)
            {
                StartCoroutine(
                    MoveToScreen(currentIndex - 1)
                );

                return;
            }
        }

        // Swipe pequeno:
        // volta para a tela atual
        StartCoroutine(
            MoveToScreen(currentIndex)
        );
    }

    // =====================================================
    // BOTÕES
    // =====================================================

    public void GoToScreen(int index)
    {
        // Índice inválido
        if (index < 0 || index >= screenPositions.Length)
            return;

        // Já está nessa tela
        if (index == currentIndex)
            return;

        // Não permite duas animações simultâneas
        if (transitioning)
            return;

        // Cancela qualquer drag que esteja acontecendo
        dragging = false;

        StartCoroutine(
            MoveToScreen(index)
        );
    }

    // =====================================================
    // MOVIMENTO ENTRE TELAS
    // =====================================================

    private IEnumerator MoveToScreen(int index)
    {
        // Segurança
        if (index < 0 || index >= screenPositions.Length)
            yield break;

        // Impede outra transição
        transitioning = true;

        float startX =
            screensContainer.anchoredPosition.x;

        // A posição que você passou:
        //
        // Tela 0 = 0
        // Tela 1 = 1518
        // Tela 2 = 3074
        // Tela 3 = 4669
        // Tela 4 = 6255
        //
        // O container precisa ir para o negativo.

        float targetX =
            -screenPositions[index];

        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / slideDuration
                );

            // Suaviza o movimento
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

            screensContainer.anchoredPosition =
                new Vector2(
                    x,
                    screensContainer.anchoredPosition.y
                );

            yield return null;
        }

        // Garante que termine exatamente na posição
        screensContainer.anchoredPosition =
            new Vector2(
                targetX,
                screensContainer.anchoredPosition.y
            );

        // Atualiza a tela atual
        currentIndex = index;

        // Libera novos inputs
        transitioning = false;
    }

    // =====================================================
    // POSIÇÃO INICIAL
    // =====================================================

    private void SetScreenInstant(int index)
    {
        index =
            Mathf.Clamp(
                index,
                0,
                screenPositions.Length - 1
            );

        currentIndex = index;

        screensContainer.anchoredPosition =
            new Vector2(
                -screenPositions[index],
                screensContainer.anchoredPosition.y
            );
    }
}