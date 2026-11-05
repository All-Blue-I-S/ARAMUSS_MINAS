using UnityEngine;

public class CameraLivre : MonoBehaviour
{
    [Header("Configurações")]
    public float velocidadeMovimento = 10f;
    public float velocidadeRapida = 25f; // Velocidade ao segurar Shift
    public float sensibilidadeRato = 2f;

    private float rotacaoX = 0f;
    private float rotacaoY = 0f;

    void Start()
    {
        // Bloqueia e esconde o cursor do rato para gravar sem distrações
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Vector3 rot = transform.localRotation.eulerAngles;
        rotacaoY = rot.y;
        rotacaoX = rot.x;
    }

    void Update()
    {
        // 1. Rotação com o Rato
        rotacaoX -= Input.GetAxis("Mouse Y") * sensibilidadeRato;
        rotacaoY += Input.GetAxis("Mouse X") * sensibilidadeRato;
        rotacaoX = Mathf.Clamp(rotacaoX, -90f, 90f); // Impede a câmara de dar cambalhotas
        transform.localRotation = Quaternion.Euler(rotacaoX, rotacaoY, 0f);

        // 2. Movimentação com WASD
        float velocidadeAtual = Input.GetKey(KeyCode.LeftShift) ? velocidadeRapida : velocidadeMovimento;
        
        float eixoZ = Input.GetAxis("Vertical"); // W e S
        float eixoX = Input.GetAxis("Horizontal"); // A e D
        float eixoY = 0f;

        // Subir (E) e Descer (Q)
        if (Input.GetKey(KeyCode.E)) eixoY = 1f;
        if (Input.GetKey(KeyCode.Q)) eixoY = -1f;

        Vector3 movimento = new Vector3(eixoX, eixoY, eixoZ) * velocidadeAtual * Time.deltaTime;
        transform.Translate(movimento, Space.Self);

        // 3. Libertar o rato (Pressione ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
