using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generic_RagDoll))]
public class AutoRagdollBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Generic_RagDoll script = (Generic_RagDoll)target;

        GUILayout.Space(20);

        if (GUILayout.Button("🛠️ AUTO GENERATE RAGDOLL", GUILayout.Height(40)))
        {
            GenerateRagdoll(script.gameObject);
        }

        if (GUILayout.Button("🧹 CLEAN RAGDOLL (Remove Physics)", GUILayout.Height(30)))
        {
            CleanRagdoll(script.gameObject);
        }
    }

    private void GenerateRagdoll(GameObject root)
    {
        Animator anim = root.GetComponentInChildren<Animator>();

        if (anim == null || !anim.isHuman)
        {
            Debug.LogError("ERRORE: Per generare il ragdoll in automatico serve un componente Animator con un Avatar Humanoid impostato.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(root, "Generate Auto Ragdoll");

        float totalMass = 80f;

        // --- 1. PRENDIAMO TUTTE LE OSSA SPECIFICHE ---
        Transform hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        Transform spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        Transform head = anim.GetBoneTransform(HumanBodyBones.Head);

        Transform neck = anim.GetBoneTransform(HumanBodyBones.Neck);
        if (neck == null) neck = head;

        Transform leftUpperArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform leftLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        Transform leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);

        Transform rightUpperArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Transform rightLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        Transform rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);

        Transform leftUpperLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform leftLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);

        Transform rightUpperLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        Transform rightLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        Transform rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);

        // Cerchiamo le dita per orientare mani e piedi
        Transform leftToes = anim.GetBoneTransform(HumanBodyBones.LeftToes);
        if (leftToes == null && leftFoot != null && leftFoot.childCount > 0) leftToes = leftFoot.GetChild(0);

        Transform rightToes = anim.GetBoneTransform(HumanBodyBones.RightToes);
        if (rightToes == null && rightFoot != null && rightFoot.childCount > 0) rightToes = rightFoot.GetChild(0);

        Transform leftHandTarget = leftHand != null && leftHand.childCount > 0 ? leftHand.GetChild(0) : null;
        Transform rightHandTarget = rightHand != null && rightHand.childCount > 0 ? rightHand.GetChild(0) : null;

        // --- 2. GENERIAMO LA FISICA ---

        // Tronco
        AddBonePhysics(hips, null, totalMass * 0.15f, false, spine);

        Transform upperTorso = spine;
        if (chest != null)
        {
            AddBonePhysics(spine, hips, totalMass * 0.10f, false, chest);
            AddBonePhysics(chest, spine, totalMass * 0.10f, false, neck);
            upperTorso = chest;
        }
        else
        {
            AddBonePhysics(spine, hips, totalMass * 0.20f, false, neck);
        }

        // Testa 
        AddBonePhysics(head, upperTorso, totalMass * 0.08f, true);

        // Braccia e Mani
        AddBonePhysics(leftUpperArm, upperTorso, totalMass * 0.05f, false, leftLowerArm);
        AddBonePhysics(leftLowerArm, leftUpperArm, totalMass * 0.04f, false, leftHand);
        AddBonePhysics(leftHand, leftLowerArm, totalMass * 0.01f, false, leftHandTarget); // Mano Sinistra

        AddBonePhysics(rightUpperArm, upperTorso, totalMass * 0.05f, false, rightLowerArm);
        AddBonePhysics(rightLowerArm, rightUpperArm, totalMass * 0.04f, false, rightHand);
        AddBonePhysics(rightHand, rightLowerArm, totalMass * 0.01f, false, rightHandTarget); // Mano Destra

        // Gambe e Piedi
        AddBonePhysics(leftUpperLeg, hips, totalMass * 0.10f, false, leftLowerLeg);
        AddBonePhysics(leftLowerLeg, leftUpperLeg, totalMass * 0.05f, false, leftFoot);
        AddBonePhysics(leftFoot, leftLowerLeg, totalMass * 0.02f, false, leftToes); // Piede Sinistro

        AddBonePhysics(rightUpperLeg, hips, totalMass * 0.10f, false, rightLowerLeg);
        AddBonePhysics(rightLowerLeg, rightUpperLeg, totalMass * 0.05f, false, rightFoot);
        AddBonePhysics(rightFoot, rightLowerLeg, totalMass * 0.02f, false, rightToes); // Piede Destro

        Debug.Log("✅ Ragdoll Generato con Successo! Aggiunte mani, piedi e busto ingrandito.");
    }

    private void AddBonePhysics(Transform bone, Transform parent, float mass, bool isHead = false, Transform targetChild = null)
    {
        if (bone == null) return;

        Rigidbody rb = bone.GetComponent<Rigidbody>();
        if (rb == null) rb = bone.gameObject.AddComponent<Rigidbody>();

        rb.mass = mass;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.isKinematic = true;

        if (isHead)
        {
            SphereCollider sphere = bone.GetComponent<SphereCollider>();
            if (sphere == null) sphere = bone.gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.12f;
            sphere.center = new Vector3(0f, 0.08f, 0f);
        }
        else
        {
            CapsuleCollider capsule = bone.GetComponent<CapsuleCollider>();
            if (capsule == null) capsule = bone.gameObject.AddComponent<CapsuleCollider>();

            if (targetChild != null)
            {
                float distance = Vector3.Distance(bone.position, targetChild.position);

                bool isTorso = bone.name.ToLower().Contains("spine") || bone.name.ToLower().Contains("chest") || bone.name.ToLower().Contains("hips");

                if (isTorso)
                {
                    // BUSTO INGRANDITO
                    capsule.radius = 0.22f; // <-- Raggio aumentato (era 0.15f)
                    capsule.height = Mathf.Max(distance * 1.5f, 0.35f); // <-- Altezza minima aumentata
                }
                else
                {
                    capsule.radius = Mathf.Max(distance * 0.25f, 0.04f);
                    capsule.height = distance;
                }

                capsule.center = bone.InverseTransformPoint(bone.position + (targetChild.position - bone.position) * 0.5f);

                capsule.direction = 0;
                Vector3 localDir = bone.InverseTransformPoint(targetChild.position).normalized;
                if (Mathf.Abs(localDir.y) > Mathf.Abs(localDir.x) && Mathf.Abs(localDir.y) > Mathf.Abs(localDir.z)) capsule.direction = 1;
                if (Mathf.Abs(localDir.z) > Mathf.Abs(localDir.x) && Mathf.Abs(localDir.z) > Mathf.Abs(localDir.y)) capsule.direction = 2;
            }
            else
            {
                // Fallback per dita e parti finali estreme
                capsule.radius = 0.06f;
                capsule.height = 0.15f;
            }
        }

        if (parent != null)
        {
            CharacterJoint joint = bone.GetComponent<CharacterJoint>();
            if (joint == null) joint = bone.gameObject.AddComponent<CharacterJoint>();

            joint.connectedBody = parent.GetComponent<Rigidbody>();
            joint.enableProjection = true;
        }
    }

    private void CleanRagdoll(GameObject root)
    {
        Animator anim = root.GetComponentInChildren<Animator>();
        if (anim == null) return;

        Undo.RegisterFullObjectHierarchyUndo(root, "Clean Ragdoll");

        CharacterJoint[] joints = root.GetComponentsInChildren<CharacterJoint>();
        foreach (var j in joints) DestroyImmediate(j);

        Rigidbody[] rbs = root.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            if (rb.gameObject != root && rb.gameObject != root.GetComponent<Rigidbody>())
                DestroyImmediate(rb);
        }

        Collider[] cols = root.GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            if (c.gameObject != root) DestroyImmediate(c);
        }

        Debug.Log("🧹 Ragdoll Pulito!");
    }
}