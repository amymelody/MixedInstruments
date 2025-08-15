using System;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using System.Collections.Generic;

#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands
{
    /// <summary>
    /// Behavior that provides events for when the system gesture starts and ends and when the
    /// menu palm pinch gesture occurs while hand tracking is in use.
    /// </summary>
    /// <remarks>
    /// See <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@1.1/manual/features/metahandtrackingaim.html">Meta Hand Tracking Aim</see>.
    /// </remarks>
    /// <seealso cref="MetaAimHand"/>
    public class MetaSystemGestureDetector : MonoBehaviour
    {
        /// <summary>
        /// The state of the system gesture.
        /// </summary>
        /// <seealso cref="systemGestureState"/>
        public enum SystemGestureState
        {
            /// <summary>
            /// The system gesture has fully ended.
            /// </summary>
            Ended,

            /// <summary>
            /// The system gesture has started or is ongoing. Typically, this means the user is looking at
            /// their palm at eye level or has not yet released the palm pinch gesture or turned their hand around.
            /// </summary>
            Started,
        }

        [SerializeField]
        InputActionProperty m_AimFlagsAction = new InputActionProperty(new InputAction(expectedControlType: "Integer"));

        /// <summary>
        /// The Input System action to read the Aim Flags.
        /// </summary>
        /// <remarks>
        /// Typically a <b>Value</b> action type with an <b>Integer</b> control type with a binding to either:
        /// <list type="bullet">
        /// <item>
        /// <description><c>&lt;MetaAimHand&gt;{LeftHand}/aimFlags</c></description>
        /// </item>
        /// <item>
        /// <description><c>&lt;MetaAimHand&gt;{RightHand}/aimFlags</c></description>
        /// </item>
        /// </list>
        /// </remarks>
        public InputActionProperty aimFlagsAction
        {
            get => m_AimFlagsAction;
            set
            {
                if (Application.isPlaying)
                    UnbindAimFlags();

                m_AimFlagsAction = value;

                if (Application.isPlaying && isActiveAndEnabled)
                    BindAimFlags();
            }
        }

        [SerializeField]
        UnityEvent m_SystemGestureStarted;

        /// <summary>
        /// Calls the methods in its invocation list when the system gesture starts, which typically occurs when
        /// the user looks at their palm at eye level.
        /// </summary>
        /// <seealso cref="systemGestureEnded"/>
        /// <seealso cref="MetaAimFlags.SystemGesture"/>
        public UnityEvent systemGestureStarted
        {
            get => m_SystemGestureStarted;
            set => m_SystemGestureStarted = value;
        }

        [SerializeField]
        UnityEvent m_SystemGestureEnded;

        /// <summary>
        /// Calls the methods in its invocation list when the system gesture ends.
        /// </summary>
        /// <remarks>
        /// This behavior postpones ending the system gesture until the user has turned their hand around.
        /// In other words, it isn't purely based on the <see cref="MetaAimFlags.SystemGesture"/>
        /// being cleared from the aim flags in order to better replicate the native visual feedback in the Meta Home menu.
        /// </remarks>
        /// <seealso cref="systemGestureStarted"/>
        /// <seealso cref="MetaAimFlags.SystemGesture"/>
        public UnityEvent systemGestureEnded
        {
            get => m_SystemGestureEnded;
            set => m_SystemGestureEnded = value;
        }

        [SerializeField]
        UnityEvent m_MenuPressed;

        /// <summary>
        /// Calls the methods in its invocation list when the menu button is triggered by a palm pinch gesture.
        /// </summary>
        /// <remarks>
        /// This is triggered by the non-dominant hand, which is the one with the menu icon (&#x2630;).
        /// The universal menu (Oculus icon) on the dominant hand does not trigger this event.
        /// </remarks>
        /// <seealso cref="MetaAimFlags.MenuPressed"/>
        public UnityEvent menuPressed
        {
            get => m_MenuPressed;
            set => m_MenuPressed = value;
        }

#if XR_HANDS_1_1_OR_NEWER
        public Handedness handedness;
#else
        public int handedness;
#endif
        public UnityEvent indexPinchStarted;
        public UnityEvent indexPinchEnded;
        public UnityEvent middlePinchStarted;
        public UnityEvent middlePinchEnded;
        public UnityEvent ringPinchStarted;
        public UnityEvent ringPinchEnded;
        public UnityEvent littlePinchStarted;
        public UnityEvent littlePinchEnded;
        public UnityEvent indexCurlStarted;
        public UnityEvent indexCurlEnded;
        public UnityEvent middleCurlStarted;
        public UnityEvent middleCurlEnded;
        public UnityEvent ringCurlStarted;
        public UnityEvent ringCurlEnded;
        public UnityEvent littleCurlStarted;
        public UnityEvent littleCurlEnded;


#if XR_HANDS_1_1_OR_NEWER
        XRHandSubsystem m_Subsystem;
        bool m_IsIndexCurling;
        bool m_IsMiddleCurling;
        bool m_IsRingCurling;
        bool m_IsLittleCurling;

        static readonly List<XRHandSubsystem> s_Subsystems = new List<XRHandSubsystem>();
#endif


        /// <summary>
        /// The state of the system gesture.
        /// </summary>
        /// <seealso cref="SystemGestureState"/>
        /// <seealso cref="systemGestureStarted"/>
        /// <seealso cref="systemGestureEnded"/>
        public IReadOnlyBindableVariable<SystemGestureState> systemGestureState => m_SystemGestureState;

        readonly BindableEnum<SystemGestureState> m_SystemGestureState = new BindableEnum<SystemGestureState>(checkEquality: false);

#if XR_HANDS_1_1_OR_NEWER && (ENABLE_VR || UNITY_GAMECORE)
        [NonSerialized] // NonSerialized is required to avoid an "Unsupported enum base type" error about the Flags enum being ulong
        MetaAimFlags m_AimFlags;
#endif

        bool m_AimFlagsBound;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            BindAimFlags();

#if XR_HANDS_1_1_OR_NEWER

            SubsystemManager.GetSubsystems(s_Subsystems);
            if (s_Subsystems.Count == 0)
                return;

            m_Subsystem = s_Subsystems[0];
            m_Subsystem.updatedHands += OnUpdatedHands;

#if ENABLE_VR || UNITY_GAMECORE
            var action = m_AimFlagsAction.action;
            if (action != null)
                // Force invoking the events upon initialization to simplify making sure the callback's desired results are synced
                UpdateAimFlags((MetaAimFlags)action.ReadValue<int>(), true);
#endif
#else
            Debug.LogWarning("Script requires XR Hands (com.unity.xr.hands) package to monitor Meta Aim Flags. Install using Window > Package Manager or click Fix on the related issue in Edit > Project Settings > XR Plug-in Management > Project Validation.", this);
            SetGestureState(SystemGestureState.Ended, true);
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            UnbindAimFlags();

#if XR_HANDS_1_1_OR_NEWER
            if (m_Subsystem == null)
                return;

            m_Subsystem.updatedHands -= OnUpdatedHands;
            m_Subsystem = null;
#endif
        }

        void BindAimFlags()
        {
            if (m_AimFlagsBound)
                return;

            var action = m_AimFlagsAction.action;
            if (action == null)
                return;

            action.performed += OnAimFlagsActionPerformedOrCanceled;
            action.canceled += OnAimFlagsActionPerformedOrCanceled;
            m_AimFlagsBound = true;

            m_AimFlagsAction.EnableDirectAction();
        }

        void UnbindAimFlags()
        {
            if (!m_AimFlagsBound)
                return;

            var action = m_AimFlagsAction.action;
            if (action == null)
                return;

            m_AimFlagsAction.DisableDirectAction();

            action.performed -= OnAimFlagsActionPerformedOrCanceled;
            action.canceled -= OnAimFlagsActionPerformedOrCanceled;
            m_AimFlagsBound = false;
        }

        void SetGestureState(SystemGestureState state, bool forceInvoke)
        {
            if (!forceInvoke && m_SystemGestureState.Value == state)
                return;

            m_SystemGestureState.Value = state;
            switch (state)
            {
                case SystemGestureState.Ended:
                    m_SystemGestureEnded?.Invoke();
                    break;
                case SystemGestureState.Started:
                    m_SystemGestureStarted?.Invoke();
                    break;
            }
        }

#if XR_HANDS_1_1_OR_NEWER && (ENABLE_VR || UNITY_GAMECORE)
        void UpdateAimFlags(MetaAimFlags value, bool forceInvoke = false)
        {
            var hadMenuPressed = (m_AimFlags & MetaAimFlags.MenuPressed) != 0;
            var hadIndexPinching = (m_AimFlags & MetaAimFlags.IndexPinching) != 0;
            m_AimFlags = value;
            var hasSystemGesture = (m_AimFlags & MetaAimFlags.SystemGesture) != 0;
            var hasMenuPressed = (m_AimFlags & MetaAimFlags.MenuPressed) != 0;
            var hasValid = (m_AimFlags & MetaAimFlags.Valid) != 0;
            var hasIndexPinching = (m_AimFlags & MetaAimFlags.IndexPinching) != 0;

            if (!hadIndexPinching && hasIndexPinching)
            {
                indexPinchStarted?.Invoke();
            }

            if (!hadMenuPressed && hasMenuPressed)
            {
                m_MenuPressed?.Invoke();
            }

            if (hasSystemGesture || hasMenuPressed)
            {
                SetGestureState(SystemGestureState.Started, forceInvoke);
                return;
            }

            if (hasValid)
            {
                SetGestureState(SystemGestureState.Ended, forceInvoke);
                return;
            }

            // We want to keep the system gesture going when the user is still index pinching
            // even though the SystemGesture flag is no longer set.
            if (hasIndexPinching && m_SystemGestureState.Value != SystemGestureState.Ended)
            {
                SetGestureState(SystemGestureState.Started, forceInvoke);
                return;
            }

            SetGestureState(SystemGestureState.Ended, forceInvoke);
        }
#endif

        void OnAimFlagsActionPerformedOrCanceled(InputAction.CallbackContext context)
        {
#if XR_HANDS_1_1_OR_NEWER && (ENABLE_VR || UNITY_GAMECORE)
            UpdateAimFlags((MetaAimFlags)context.ReadValue<int>());
#endif
        }


#if XR_HANDS_1_1_OR_NEWER
        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            var wasIndexCurling = m_IsIndexCurling;
            var wasMiddleCurling = m_IsMiddleCurling;
            var wasRingCurling = m_IsRingCurling;
            var wasLittleCurling = m_IsLittleCurling;
            switch (handedness)
            {
                case Handedness.Left:
                    if (!HasUpdateSuccessFlag(updateSuccessFlags, XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints))
                        return;

                    var leftHand = subsystem.leftHand;
                    m_IsIndexCurling = IsIndexGrabbing(leftHand);
                    m_IsMiddleCurling = IsMiddleGrabbing(leftHand);
                    m_IsRingCurling = IsRingGrabbing(leftHand);
                    m_IsLittleCurling = IsLittleGrabbing(leftHand);
                    break;
                case Handedness.Right:
                    if (!HasUpdateSuccessFlag(updateSuccessFlags, XRHandSubsystem.UpdateSuccessFlags.RightHandJoints))
                        return;

                    var rightHand = subsystem.rightHand;
                    m_IsIndexCurling = IsIndexGrabbing(rightHand);
                    m_IsMiddleCurling = IsMiddleGrabbing(rightHand);
                    m_IsRingCurling = IsRingGrabbing(rightHand);
                    m_IsLittleCurling = IsLittleGrabbing(rightHand);
                    break;
            }
        }

        /// <summary>
        /// Determines whether one or more bit fields are set in the flags.
        /// Non-boxing version of <c>HasFlag</c> for <see cref="XRHandSubsystem.UpdateSuccessFlags"/>.
        /// </summary>
        /// <param name="successFlags">The flags enum instance.</param>
        /// <param name="successFlag">The flag to check if set.</param>
        /// <returns>Returns <see langword="true"/> if the bit field or bit fields are set, otherwise returns <see langword="false"/>.</returns>
        static bool HasUpdateSuccessFlag(XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateSuccessFlags successFlag)
        {
            return (successFlags & successFlag) == successFlag;
        }

        /// <summary>
        /// Returns true if the given hand's index finger tip is closer to the wrist than the index proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's index finger tip is closer to the wrist than the index proximal joint, false otherwise.</returns>
        static bool IsIndexGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.IndexProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's middle finger tip is closer to the wrist than the middle proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's middle finger tip is closer to the wrist than the middle proximal joint, false otherwise.</returns>
        static bool IsMiddleGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.MiddleProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's ring finger tip is closer to the wrist than the ring proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's ring finger tip is closer to the wrist than the ring proximal joint, false otherwise.</returns>
        static bool IsRingGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.RingTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.RingProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's little finger tip is closer to the wrist than the little proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's little finger tip is closer to the wrist than the little proximal joint, false otherwise.</returns>
        static bool IsLittleGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.LittleProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }
#endif
    }
}
