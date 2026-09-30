namespace AlmaDino.Core.Interfaces
{
    /// <summary>
    /// Marca superficies elásticas o trampolines (hongos rebotadores, ramas elásticas)
    /// para que el detector de suelo del jugador las diferencie de suelo firme.
    /// Esto evita que el contacto con el hongo corte prematuramente los saltos o cancele el impulso.
    /// </summary>
    public interface IBouncySurface2D
    {
    }
}
