#!/usr/bin/env python3
"""
Genera un hash BCrypt compatible con BCrypt.Net-Next (el que usa
AuthCredentials:PasswordHash en la API) a partir de una contraseña en
texto plano que TÚ eliges. El hash resultante es lo único que se guarda
en configuración/variables de entorno; la contraseña en texto plano no
queda guardada en ningún archivo.

Uso:
    pip install bcrypt --break-system-packages   # si no lo tienes instalado
    python3 tools/generate_password_hash.py
"""
import getpass

try:
    import bcrypt
except ImportError:
    raise SystemExit(
        "Falta el paquete 'bcrypt'. Instálalo con:\n"
        "  pip install bcrypt --break-system-packages\n"
    )


def main() -> None:
    password = getpass.getpass("Contraseña para el usuario admin (no se mostrará en pantalla): ")
    confirm = getpass.getpass("Confírmala: ")

    if password != confirm:
        raise SystemExit("Las contraseñas no coinciden. Intenta de nuevo.")
    if len(password) < 8:
        raise SystemExit("Usa al menos 8 caracteres.")

    hashed = bcrypt.hashpw(password.encode("utf-8"), bcrypt.gensalt(rounds=11)).decode("utf-8")

    print("\nCopia este valor en AuthCredentials:PasswordHash")
    print("(appsettings.Development.json local, o la variable de entorno AuthCredentials__PasswordHash):\n")
    print(hashed)


if __name__ == "__main__":
    main()
