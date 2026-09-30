namespace AlmaDino.Core.Interfaces
{
    /// <summary>
    /// Interfaz para entidades que pueden ser catapultadas o rebotadas por elementos del entorno
    /// (como hongos rebotadores o ramas elásticas).
    /// </summary>
    public interface IBounceable2D
    {
        /// <summary>
        /// Aplica una fuerza/velocidad vertical de rebote a la entidad.
        /// </summary>
        /// <param name="verticalVelocity">Magnitud de velocidad vertical en m/s.</param>
        /// <param name="refreshAirAbilities">Si debe recargar habilidades aéreas como el Doble Salto.</param>
        void ApplyBounce(float verticalVelocity, bool refreshAirAbilities);
    }
}
