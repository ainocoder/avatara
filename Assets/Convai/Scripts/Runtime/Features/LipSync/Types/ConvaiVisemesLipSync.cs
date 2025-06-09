using System.Collections.Generic;
using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.Extensions;
using Convai.Scripts.Runtime.Features.LipSync.Models;
using Convai.Scripts.Runtime.Features.LipSync.Visemes;
using Convai.Scripts.Runtime.LoggerSystem;
using Service;
using UnityEngine;

namespace Convai.Scripts.Runtime.Features.LipSync.Types
{
    public class ConvaiVisemesLipSync : ConvaiLipSyncApplicationBase
    {
        private const float FRAMERATE = 1f / 100.0f;
        private readonly Viseme _defaultViseme = new();
        private Viseme _currentViseme;
        private Queue<Queue<VisemesData>> _visemesDataQueue = new();

        private void LateUpdate()
        {
            // Check if the dequeued frame is not null.
            if (_currentViseme == null) return;
            // Check if the frame represents silence (-2 is a placeholder for silence).
            if (Mathf.Approximately(_currentViseme.Sil, -2)) return;


            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, -5.0f));

            if (HasHeadSkinnedMeshRenderer)
                UpdateMeshRenderer(FacialExpressionData.Head);
            if (HasTeethSkinnedMeshRenderer)
                UpdateMeshRenderer(FacialExpressionData.Teeth);
            if (HasTongueSkinnedMeshRenderer)
                UpdateMeshRenderer(FacialExpressionData.Tongue);

            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f - CalculateBoneEffect(FacialExpressionData.JawBoneEffector) * 30f));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, CalculateBoneEffect(FacialExpressionData.TongueBoneEffector) * 80f - 5f));
        }

        public override void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            base.Initialize(convaiLipSync, convaiNPC);
            InvokeRepeating(nameof(UpdateBlendShape), 0, FRAMERATE);
        }

        public override void ClearQueue()
        {
            _visemesDataQueue = new Queue<Queue<VisemesData>>();
            _currentViseme = new Viseme();
        }

        public override void PurgeExcessBlendShapeFrames()
        {
            if (_visemesDataQueue.Count == 0) return;
            if (!CanPurge(_visemesDataQueue.Peek())) return;
            ConvaiLogger.Info($"Purging {_visemesDataQueue.Peek().Count} Frames", ConvaiLogger.LogCategory.LipSync);
            _visemesDataQueue.Dequeue();
        }

        public override void EnqueueQueue(Queue<VisemesData> visemesFrames)
        {
            _visemesDataQueue.Enqueue(visemesFrames);
        }

        public override void EnqueueFrame(VisemesData viseme)
        {
            if (_visemesDataQueue.Count == 0) EnqueueQueue(new Queue<VisemesData>());
            _visemesDataQueue.Peek().Enqueue(viseme);
        }


        protected void UpdateBlendShape()
        {
            if (_visemesDataQueue is not { Count: > 0 })
            {
                _currentViseme = _defaultViseme;
                return;
            }

            // Dequeue the next frame of visemes data from the faceDataList.
            if (_visemesDataQueue.Peek() == null || _visemesDataQueue.Peek().Count <= 0)
            {
                _visemesDataQueue.Dequeue();
                return;
            }

            if (!ConvaiNPC.IsCharacterTalking) return;

            _currentViseme = _visemesDataQueue.Peek().Dequeue().Visemes;
        }

        private float CalculateBoneEffect(VisemeBoneEffectorList boneEffectorList)
        {
            if (boneEffectorList is null) return 0;
            return (
                       boneEffectorList.sil * _currentViseme.Sil +
                       boneEffectorList.pp * _currentViseme.Pp +
                       boneEffectorList.ff * _currentViseme.Ff +
                       boneEffectorList.th * _currentViseme.Th +
                       boneEffectorList.dd * _currentViseme.Dd +
                       boneEffectorList.kk * _currentViseme.Kk +
                       boneEffectorList.ch * _currentViseme.Ch +
                       boneEffectorList.ss * _currentViseme.Ss +
                       boneEffectorList.nn * _currentViseme.Nn +
                       boneEffectorList.rr * _currentViseme.Rr +
                       boneEffectorList.aa * _currentViseme.Aa +
                       boneEffectorList.e * _currentViseme.E +
                       boneEffectorList.ih * _currentViseme.Ih +
                       boneEffectorList.oh * _currentViseme.Oh +
                       boneEffectorList.ou * _currentViseme.Ou
                   )
                   / boneEffectorList.Total;
        }

        // 추가된 매핑 메서드
        private Dictionary<int, int> GetBlendShapeMapping(SkinnedMeshRenderer skinnedMesh)
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();

            // Coyote 메시인 경우 특별한 매핑 사용
            if (skinnedMesh.name.Contains("Coyote") || skinnedMesh.sharedMesh.name.Contains("Coyote"))
            {
                // V_Lip_Open 매핑 추가
                mapping[7] = 8;     // V_Lip_Open -> V_Lip_Open

                // 핵심 매핑 (오류 해결용)
                mapping[114] = 33;  // Mouth_Shrug_Upper -> Mouth_Shrug_Upper
                mapping[115] = 34;  // Mouth_Shrug_Lower -> Mouth_Shrug_Lower
                mapping[116] = 35;  // Mouth_Drop_Upper -> Mouth_Up_Upper_L (또는 36 Mouth_Up_Upper_R)
                mapping[117] = 37;  // Mouth_Drop_Lower -> Mouth_Down_Lower_L (또는 38 Mouth_Down_Lower_R)
                mapping[123] = 39;  // Mouth_Close -> Mouth_Close
                mapping[127] = 45;  // Jaw_Open -> Jaw_Open

                // 추가 매핑 (더 자연스러운 립싱크를 위함)
                mapping[66] = 23;   // Mouth_Smile_L -> Mouth_Smile_L
                mapping[67] = 24;   // Mouth_Smile_R -> Mouth_Smile_R
                mapping[70] = 25;   // Mouth_Frown_L -> Mouth_Frown_L
                mapping[71] = 26;   // Mouth_Frown_R -> Mouth_Frown_R
                mapping[76] = 27;   // Mouth_Press_L -> Mouth_Press_L
                mapping[77] = 28;   // Mouth_Press_R -> Mouth_Press_R
                mapping[82] = 29;   // Mouth_Pucker_Up_L -> Mouth_Pucker
                mapping[83] = 29;   // Mouth_Pucker_Up_R -> Mouth_Pucker
                mapping[86] = 30;   // Mouth_Funnel_Up_L -> Mouth_Funnel
                mapping[87] = 30;   // Mouth_Funnel_Up_R -> Mouth_Funnel
                mapping[125] = 40;  // Tongue_Bulge_L -> Tongue_Tip_Up
                mapping[126] = 40;  // Tongue_Bulge_R -> Tongue_Tip_Up
            }

            return mapping;
        }

        private void UpdateMeshRenderer(SkinMeshRendererData data)
        {
            VisemeEffectorsList effectorsList = data.VisemeEffectorsList;
            SkinnedMeshRenderer skinnedMesh = data.Renderer;
            Vector2 bounds = data.WeightBounds;
            if (effectorsList == null) return;

            // 매핑 테이블 가져오기
            Dictionary<int, int> blendShapeMapping = GetBlendShapeMapping(skinnedMesh);

            Dictionary<int, float> finalModifiedValuesDictionary = new();
            CalculateBlendShapeEffect(effectorsList.pp, ref finalModifiedValuesDictionary, _currentViseme.Pp);
            CalculateBlendShapeEffect(effectorsList.ff, ref finalModifiedValuesDictionary, _currentViseme.Ff);
            CalculateBlendShapeEffect(effectorsList.th, ref finalModifiedValuesDictionary, _currentViseme.Th);
            CalculateBlendShapeEffect(effectorsList.dd, ref finalModifiedValuesDictionary, _currentViseme.Dd);
            CalculateBlendShapeEffect(effectorsList.kk, ref finalModifiedValuesDictionary, _currentViseme.Kk);
            CalculateBlendShapeEffect(effectorsList.ch, ref finalModifiedValuesDictionary, _currentViseme.Ch);
            CalculateBlendShapeEffect(effectorsList.ss, ref finalModifiedValuesDictionary, _currentViseme.Ss);
            CalculateBlendShapeEffect(effectorsList.nn, ref finalModifiedValuesDictionary, _currentViseme.Nn);
            CalculateBlendShapeEffect(effectorsList.rr, ref finalModifiedValuesDictionary, _currentViseme.Rr);
            CalculateBlendShapeEffect(effectorsList.aa, ref finalModifiedValuesDictionary, _currentViseme.Aa);
            CalculateBlendShapeEffect(effectorsList.e, ref finalModifiedValuesDictionary, _currentViseme.E);
            CalculateBlendShapeEffect(effectorsList.ih, ref finalModifiedValuesDictionary, _currentViseme.Ih);
            CalculateBlendShapeEffect(effectorsList.oh, ref finalModifiedValuesDictionary, _currentViseme.Oh);
            CalculateBlendShapeEffect(effectorsList.ou, ref finalModifiedValuesDictionary, _currentViseme.Ou);

            // 매핑 적용 및 안전 검사
            foreach (KeyValuePair<int, float> keyValuePair in finalModifiedValuesDictionary)
            {
                int blendShapeIndex = keyValuePair.Key;
                float blendShapeValue = keyValuePair.Value * bounds.y - bounds.x;

                // 매핑된 인덱스가 있으면 사용
                if (blendShapeMapping.ContainsKey(blendShapeIndex))
                    blendShapeIndex = blendShapeMapping[blendShapeIndex];

                // 안전 검사
                if (blendShapeIndex < skinnedMesh.sharedMesh.blendShapeCount)
                    skinnedMesh.SetBlendShapeWeightInterpolate(blendShapeIndex, blendShapeValue, WeightBlendingPower);
            }
        }

        private static void CalculateBlendShapeEffect(List<BlendShapesIndexEffector> effectors, ref Dictionary<int, float> dictionary, float value)
        {
            foreach (BlendShapesIndexEffector blendShapesIndexEffector in effectors)
                if (dictionary.ContainsKey(blendShapesIndexEffector.index))
                    dictionary[blendShapesIndexEffector.index] += value * blendShapesIndexEffector.effectPercentage;
                else
                    dictionary[blendShapesIndexEffector.index] = value * blendShapesIndexEffector.effectPercentage;
        }
    }
}
