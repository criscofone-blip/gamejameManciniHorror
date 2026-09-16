using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Copia i Transform (posizione/rotazione/scala locali) da una scena "sorgente"
/// a una scena "destinazione", abbinando gli oggetti per percorso nella gerarchia.
/// Utile quando due scene hanno la stessa struttura ma dati diversi
/// (es. una ha i collider giusti, l'altra le posizioni giuste).
/// </summary>
public class TransformTransferWindow : EditorWindow
{
    private SceneAsset sourceScene;   // scena con le POSIZIONI corrette
    private SceneAsset targetScene;   // scena da correggere (quella con i collider)

    private bool copyPosition = true;
    private bool copyRotation = true;
    private bool copyScale = true;

    private Vector2 scroll;
    private string report = "Nessuna analisi eseguita.";

    private const int MaxListed = 80;

    [MenuItem("Tools/Transfer Transforms")]
    public static void Open()
    {
        GetWindow<TransformTransferWindow>("Transfer Transforms");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Copia Transform tra due scene", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        sourceScene = (SceneAsset)EditorGUILayout.ObjectField(
            "Sorgente (posizioni OK)", sourceScene, typeof(SceneAsset), false);

        targetScene = (SceneAsset)EditorGUILayout.ObjectField(
            "Destinazione (con collider)", targetScene, typeof(SceneAsset), false);

        EditorGUILayout.Space();
        copyPosition = EditorGUILayout.Toggle("Copia Position", copyPosition);
        copyRotation = EditorGUILayout.Toggle("Copia Rotation", copyRotation);
        copyScale = EditorGUILayout.Toggle("Copia Scale", copyScale);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "1) Fai un commit/backup PRIMA di applicare.\n" +
            "2) Lancia sempre prima l'analisi (dry run).\n" +
            "3) Dopo 'Applica' la scena resta NON salvata: controlla e poi salva con Ctrl+S.",
            MessageType.Warning);

        using (new EditorGUI.DisabledScope(sourceScene == null || targetScene == null))
        {
            if (GUILayout.Button("1) Analizza (dry run) — non modifica nulla"))
                Run(false);

            if (GUILayout.Button("2) Applica (copia i Transform)"))
                Run(true);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Report", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void Run(bool apply)
    {
        string srcPath = AssetDatabase.GetAssetPath(sourceScene);
        string dstPath = AssetDatabase.GetAssetPath(targetScene);

        if (srcPath == dstPath)
        {
            report = "Sorgente e destinazione sono la stessa scena. Scegline due diverse.";
            return;
        }

        // Chiede di salvare eventuali modifiche in sospeso prima di cambiare scena.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene dst = EditorSceneManager.OpenScene(dstPath, OpenSceneMode.Single);
        Scene src = EditorSceneManager.OpenScene(srcPath, OpenSceneMode.Additive);

        Dictionary<string, Transform> srcMap = new Dictionary<string, Transform>();
        Dictionary<string, Transform> dstMap = new Dictionary<string, Transform>();

        Collect(src, srcMap);
        Collect(dst, dstMap);

        int matched = 0;
        List<string> missingInSource = new List<string>();

        foreach (KeyValuePair<string, Transform> pair in dstMap)
        {
            if (!srcMap.TryGetValue(pair.Key, out Transform s))
            {
                missingInSource.Add(pair.Key);
                continue;
            }

            matched++;

            if (!apply)
                continue;

            Transform d = pair.Value;
            Undo.RecordObject(d, "Transfer Transforms");

            if (copyPosition) d.localPosition = s.localPosition;
            if (copyRotation) d.localRotation = s.localRotation;
            if (copyScale) d.localScale = s.localScale;

            EditorUtility.SetDirty(d);
        }

        List<string> missingInTarget = new List<string>();
        foreach (KeyValuePair<string, Transform> pair in srcMap)
        {
            if (!dstMap.ContainsKey(pair.Key))
                missingInTarget.Add(pair.Key);
        }

        // Chiude la scena sorgente aperta in additive.
        EditorSceneManager.CloseScene(src, true);

        if (apply)
            EditorSceneManager.MarkSceneDirty(dst);

        report = BuildReport(apply, srcMap.Count, dstMap.Count, matched, missingInSource, missingInTarget);
    }

    private string BuildReport(bool apply, int srcCount, int dstCount, int matched,
                               List<string> missingInSource, List<string> missingInTarget)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(apply
            ? "=== APPLICATO (scena NON ancora salvata: controlla e premi Ctrl+S) ==="
            : "=== DRY RUN — nessuna modifica effettuata ===");
        sb.AppendLine();
        sb.AppendLine($"Oggetti nella sorgente:    {srcCount}");
        sb.AppendLine($"Oggetti nella destinazione:{dstCount}");
        sb.AppendLine($"Abbinati correttamente:    {matched}");
        sb.AppendLine();

        sb.AppendLine($"Presenti in DESTINAZIONE ma non trovati in sorgente: {missingInSource.Count}");
        AppendList(sb, missingInSource);
        sb.AppendLine();

        sb.AppendLine($"Presenti in SORGENTE ma non trovati in destinazione: {missingInTarget.Count}");
        AppendList(sb, missingInTarget);

        return sb.ToString();
    }

    private void AppendList(StringBuilder sb, List<string> items)
    {
        int shown = Mathf.Min(items.Count, MaxListed);

        for (int i = 0; i < shown; i++)
            sb.AppendLine("   - " + items[i]);

        if (items.Count > shown)
            sb.AppendLine($"   ... e altri {items.Count - shown}");
    }

    // ---------- Raccolta oggetti con percorso gerarchico ----------

    private static void Collect(Scene scene, Dictionary<string, Transform> map)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        List<Transform> rootTransforms = new List<Transform>(roots.Length);
        foreach (GameObject go in roots)
            rootTransforms.Add(go.transform);

        CollectLevel(rootTransforms, string.Empty, map);
    }

    /// <summary>
    /// Costruisce il percorso di ogni oggetto. Se più fratelli hanno lo stesso nome
    /// (cosa frequente nei modelli importati), aggiunge #indice per distinguerli.
    /// </summary>
    private static void CollectLevel(List<Transform> siblings, string parentPath, Dictionary<string, Transform> map)
    {
        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (Transform t in siblings)
        {
            nameCount.TryGetValue(t.name, out int c);
            nameCount[t.name] = c + 1;
        }

        Dictionary<string, int> seen = new Dictionary<string, int>();

        foreach (Transform t in siblings)
        {
            string key = t.name;

            if (nameCount[t.name] > 1)
            {
                seen.TryGetValue(t.name, out int idx);
                key = $"{t.name}#{idx}";
                seen[t.name] = idx + 1;
            }

            string path = string.IsNullOrEmpty(parentPath) ? key : parentPath + "/" + key;
            map[path] = t;

            if (t.childCount == 0)
                continue;

            List<Transform> children = new List<Transform>(t.childCount);
            for (int i = 0; i < t.childCount; i++)
                children.Add(t.GetChild(i));

            CollectLevel(children, path, map);
        }
    }
}
