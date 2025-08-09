using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Simple highlighter that appends a secondary outline material to the target renderers when enabled via SetHighlighted(true).
    /// KISS: Inspector-first, no runtime material mutation except swapping the renderer.materials array.
    /// Works with URP (assign a URP-compatible outline material in the Inspector).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Spookie/Outline Highlighter")]
    public class OutlineHighlighter : MonoBehaviour
    {
        [Header("Target Renderers")]
        [Tooltip("Renderers to highlight. If empty, will auto-collect MeshRenderer/SkinnedMeshRenderer in children at Awake.")]
        [SerializeField] private List<Renderer> targetRenderers = new List<Renderer>();

        [Header("Outline Material (URP)")]
        [Tooltip("URP material used as a second material to create an outline.")]
        [SerializeField] private Material outlineMaterial;

        [Header("Options")]
        [Tooltip("If true, caches the original materials per renderer at Awake and restores them on unhighlight.")]
        [SerializeField] private bool restoreOriginalOnUnhighlight = true;

        // Cache
        private readonly List<Material[]> _originalMats = new List<Material[]>();
        private bool _highlighted;

        private void Awake()
        {
            if (targetRenderers == null || targetRenderers.Count == 0)
            {
                targetRenderers = new List<Renderer>();
                GetComponentsInChildren(true, targetRenderers);
                // Filter to MeshRenderers/SkinnedMeshRenderers only
                for (int i = targetRenderers.Count - 1; i >= 0; i--)
                {
                    if (!(targetRenderers[i] is MeshRenderer) && !(targetRenderers[i] is SkinnedMeshRenderer))
                    {
                        targetRenderers.RemoveAt(i);
                    }
                }
            }

            _originalMats.Clear();
            if (restoreOriginalOnUnhighlight)
            {
                foreach (var r in targetRenderers)
                {
                    _originalMats.Add(r != null ? r.materials : Array.Empty<Material>());
                }
            }
        }

        private void OnDisable()
        {
            // Safety: clear highlight when disabled
            if (_highlighted)
            {
                SetHighlighted(false);
            }
        }

        public void SetHighlighted(bool enabled)
        {
            if (_highlighted == enabled) return;
            _highlighted = enabled;

            if (outlineMaterial == null || targetRenderers == null || targetRenderers.Count == 0)
            {
                return;
            }

            if (enabled)
            {
                for (int i = 0; i < targetRenderers.Count; i++)
                {
                    var r = targetRenderers[i];
                    if (r == null) continue;

                    // Use renderer.materials to get instance list (avoids touching sharedMaterials)
                    var mats = r.materials;
                    // Avoid duplicates
                    bool has = false;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] == outlineMaterial) { has = true; break; }
                    }
                    if (!has)
                    {
                        Array.Resize(ref mats, mats.Length + 1);
                        mats[mats.Length - 1] = outlineMaterial;
                        r.materials = mats;
                    }
                }
            }
            else
            {
                for (int i = 0; i < targetRenderers.Count; i++)
                {
                    var r = targetRenderers[i];
                    if (r == null) continue;

                    if (restoreOriginalOnUnhighlight && i < _originalMats.Count && _originalMats[i] != null && _originalMats[i].Length > 0)
                    {
                        r.materials = _originalMats[i];
                    }
                    else
                    {
                        var mats = r.materials;
                        // Remove any occurrences of the outline material
                        int count = 0;
                        for (int m = 0; m < mats.Length; m++)
                        {
                            if (mats[m] != outlineMaterial) count++;
                        }
                        if (count != mats.Length)
                        {
                            var newMats = new Material[count];
                            int w = 0;
                            for (int m = 0; m < mats.Length; m++)
                            {
                                if (mats[m] == outlineMaterial) continue;
                                newMats[w++] = mats[m];
                            }
                            r.materials = newMats;
                        }
                    }
                }
            }
        }

        private void OnValidate()
        {
            // Clean nulls from the list to keep it tidy in Inspector
            if (targetRenderers != null)
            {
                for (int i = targetRenderers.Count - 1; i >= 0; i--)
                {
                    if (targetRenderers[i] == null) targetRenderers.RemoveAt(i);
                }
            }
        }
    }
}

// ScriptRole | RelatedScripts | UsesSO | ReceivesFrom | SendsTo
// ScriptRole: Adds/removes an outline material to the object's renderers to visually highlight interactables.
// RelatedScripts: PlayerController.PlayerInteraction
// UsesSO: No
// ReceivesFrom: PlayerInteraction (SetHighlighted calls)
// SendsTo: Renderer.materials arrays (secondary material)
// Attach to: Interactable object root. Assign Outline Material (URP). Optionally populate Target Renderers; otherwise auto-collect child mesh renderers at Awake.
