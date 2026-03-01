using UnityEngine;

namespace FirstPersonSystem
{
    public class Footsteps : MonoBehaviour
    {
        [Header("Footstep Parameters")]
        [SerializeField] private bool useFootsteps = true;

        public GroundType defaultGroundType;
        [SerializeField] private float baseStepSpeed = 0.5f;
        [SerializeField] private float crouchStepMultipleir = 1.5f;
        [SerializeField] private float sprintStepMultipleir = 0.6f;
        [SerializeField, Range(0f, 1f)] private float minPitch = 0.9f;
        [SerializeField, Range(1f, 2f)] private float maxPitch = 1.1f;
        [SerializeField] private AudioSource footstepAudioSource = default;
        private float footstepTimer = 0f;
        private float GetCurrentOffset => _controller.IsCrouching ? baseStepSpeed * crouchStepMultipleir : _controller.IsSprinting ? baseStepSpeed * sprintStepMultipleir : baseStepSpeed;
        private FirstPersonController _controller;
        private AudioClip previousClip;

        private void Start()
        {
            _controller = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            if (useFootsteps)
                HandleFootsteps();
        }

        private void HandleFootsteps()
        {
            if (!_controller.IsGrounded) return;
            if (_controller.CurrentInput == Vector2.zero) return;

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                PlaySoundWithRandomPitch(GetClipFromArray(defaultGroundType.walkAudioClips));
                footstepTimer = GetCurrentOffset;
            }
        }

        private AudioClip GetClipFromArray(AudioClip[] clipArray)
        {
            if (clipArray == null || clipArray.Length == 0) return null;

            int attemps = 3;
            AudioClip selectedClip = clipArray[Random.Range(0, clipArray.Length)];

            while (selectedClip == previousClip && attemps > 0)
            {
                selectedClip = clipArray[Random.Range(0, clipArray.Length)];
                attemps--;
            }

            previousClip = selectedClip;
            return selectedClip;
        }

        private void PlaySoundWithRandomPitch(AudioClip clip)
        {
            if (clip == null) return;
            footstepAudioSource.pitch = Random.Range(minPitch, maxPitch);
            footstepAudioSource.PlayOneShot(clip);
        }
    }
}