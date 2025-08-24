namespace Game.Save
{
    /// <summary>
    /// Interfaz para sistemas que aportan datos a SaveData. Registro explícito en SaveGameManager.
    /// </summary>
    public interface ISaveParticipant
    {
        /// <summary>Orden opcional (menor = antes). Usar 0 por defecto.</summary>
        int Order => 0;
        void WriteTo(SaveData data);
        void ReadFrom(SaveData data);
    }
}

// ScriptRole: Contrato de participación en guardado/carga.
// RelatedScripts: SaveGameManager
// UsesSO: No
// ReceivesFrom: SaveGameManager (callbacks)
// SendsTo: SaveData (mutación)
// Adjuntar: No aplica.