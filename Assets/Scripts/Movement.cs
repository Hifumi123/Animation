using Cinemachine;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float turnSpeed = 20;

    public float moveSpeedScale = 0.0005f;

    public Transform startPointOfSword;

    public Transform endPointofSword;

    public Transform stone;

    public ParticleSystem spark;

    private Animator m_Animator;

    private Rigidbody m_Rigidbody;

    private CinemachineImpulseSource m_ImpulseSource;

    private bool m_IsMoving = false;

    private bool m_IsAttacking = false;

    private Vector3 m_Movement = Vector3.zero;

    private Quaternion m_Rotation = Quaternion.identity;

    private float m_Speed = 0;

    private float m_AttackAnimationTotalTime = 2.23f;

    private float m_AttackAnimationTime = 0;

    void Start()
    {
        m_Animator = GetComponent<Animator>();

        m_Rigidbody = GetComponent<Rigidbody>();

        m_ImpulseSource = GetComponent<CinemachineImpulseSource>();

        spark.Stop();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        m_IsMoving = !Mathf.Approximately(h, 0) || !Mathf.Approximately(v, 0);

        m_Animator.SetBool("IsMoving", m_IsMoving);

        if (m_IsMoving)
        {
            m_Movement.Set(h, 0, v);
            m_Movement.Normalize();

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0);
            m_Rotation = Quaternion.LookRotation(desiredForward);

            float distance = Mathf.Sqrt(h * h + v * v);
            m_Speed = distance / Time.deltaTime;

            m_Animator.SetFloat("MoveSpeed", m_Speed);

            m_Speed *= moveSpeedScale;

            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Speed);
            m_Rigidbody.MoveRotation(m_Rotation);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            m_IsAttacking = true;

            m_Animator.SetTrigger("TriggerAttack");
        }

        if (m_IsAttacking)
        {
            m_AttackAnimationTime += Time.fixedDeltaTime;

            RaycastHit hit;

            if (Physics.Linecast(startPointOfSword.position, endPointofSword.position, out hit))
                if (hit.transform == stone)
                {
                    spark.transform.position = hit.point;
                    spark.transform.LookAt(hit.point + hit.normal);
                    spark.Play();

                    m_ImpulseSource.GenerateImpulse();
                }
        }

        if (m_AttackAnimationTime >= m_AttackAnimationTotalTime)
        {
            m_IsAttacking = false;

            m_AttackAnimationTime = 0;
        }

        if (!m_IsMoving)
        {
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }
}
