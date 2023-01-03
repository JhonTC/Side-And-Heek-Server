using Riptide;
using UnityEngine;

public static class MessageExtensions
{
    #region Vector2
    /// <inheritdoc cref="Add(Message, Vector2)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Vector2)"/> and simply provides an alternative type-explicit way to add a <see cref="Vector2"/> to the message.</remarks>
    public static Message AddVector2(this Message message, Vector2 value) => Add(message, value);

    /// <summary>Adds a <see cref="Vector2"/> to the message.</summary>
    /// <param name="value">The <see cref="Vector2"/> to add.</param>
    /// <returns>The message that the <see cref="Vector2"/> was added to.</returns>
    public static Message Add(this Message message, Vector2 value)
    {
        message.AddFloat(value.x);
        message.AddFloat(value.y);
        return message;
    }

    /// <summary>Retrieves a <see cref="Vector2"/> from the message.</summary>
    /// <returns>The <see cref="Vector2"/> that was retrieved.</returns>
    public static Vector2 GetVector2(this Message message)
    {
        return new Vector2(message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Vector3
    /// <inheritdoc cref="Add(Message, Vector3)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Vector3)"/> and simply provides an alternative type-explicit way to add a <see cref="Vector3"/> to the message.</remarks>
    public static Message AddVector3(this Message message, Vector3 value) => Add(message, value);

    /// <summary>Adds a <see cref="Vector3"/> to the message.</summary>
    /// <param name="value">The <see cref="Vector3"/> to add.</param>
    /// <returns>The message that the <see cref="Vector3"/> was added to.</returns>
    public static Message Add(this Message message, Vector3 value)
    {
        message.AddFloat(value.x);
        message.AddFloat(value.y);
        message.AddFloat(value.z);
        return message;
    }

    /// <summary>Retrieves a <see cref="Vector3"/> from the message.</summary>
    /// <returns>The <see cref="Vector3"/> that was retrieved.</returns>
    public static Vector3 GetVector3(this Message message)
    {
        return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Quaternion
    /// <inheritdoc cref="Add(Message, Quaternion)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Quaternion)"/> and simply provides an alternative type-explicit way to add a <see cref="Quaternion"/> to the message.</remarks>
    public static Message AddQuaternion(this Message message, Quaternion value) => Add(message, value);

    /// <summary>Adds a <see cref="Quaternion"/> to the message.</summary>
    /// <param name="value">The <see cref="Quaternion"/> to add.</param>
    /// <returns>The message that the <see cref="Quaternion"/> was added to.</returns>
    public static Message Add(this Message message, Quaternion value)
    {
        message.AddFloat(value.x);
        message.AddFloat(value.y);
        message.AddFloat(value.z);
        message.AddFloat(value.w);
        return message;
    }

    /// <summary>Retrieves a <see cref="Quaternion"/> from the message.</summary>
    /// <returns>The <see cref="Quaternion"/> that was retrieved.</returns>
    public static Quaternion GetQuaternion(this Message message)
    {
        return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region Colour
    /// <inheritdoc cref="Add(Message, Color)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, Color)"/> and simply provides an alternative type-explicit way to add a <see cref="Vector3"/> to the message.</remarks>
    public static Message AddColour(this Message message, Color value) => Add(message, value);

    /// <summary>Adds a <see cref="Color"/> to the message.</summary>
    /// <param name="value">The <see cref="Color"/> to add.</param>
    /// <returns>The message that the <see cref="Color"/> was added to.</returns>
    public static Message Add(this Message message, Color value)
    {
        message.AddFloat(value.r);
        message.AddFloat(value.g);
        message.AddFloat(value.b);
        message.AddFloat(value.a);
        return message;
    }

    /// <summary>Retrieves a <see cref="Color"/> from the message.</summary>
    /// <returns>The <see cref="Color"/> that was retrieved.</returns>
    public static Color GetColour(this Message message)
    {
        return new Color(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
    #endregion

    #region GameRules
    /// <inheritdoc cref="Add(Message, GameRules)"/>
    /// <remarks>Relying on the correct Add overload being chosen based on the parameter type can increase the odds of accidental type mismatches when retrieving data from a message. This method calls <see cref="Add(Message, GameRules)"/> and simply provides an alternative type-explicit way to add a <see cref="Vector3"/> to the message.</remarks>
    public static Message AddGameRules(this Message message, GameRules value) => Add(message, value);

    /// <summary>Adds a <see cref="GameRules"/> to the message.</summary>
    /// <param name="value">The <see cref="GameRules"/> to add.</param>
    /// <returns>The message that the <see cref="GameRules"/> was added to.</returns>
    public static Message Add(this Message message, GameRules value)
    {
        return value.AddMessageValues(message);
    }

    /// <summary>Retrieves a <see cref="GameRules"/> from the message.</summary>
    /// <returns>The <see cref="GameRules"/> that was retrieved.</returns>
    public static GameRules GetGameRules(this Message message)
    {
        GameRules gamerules = GameRules.CreateGameRulesFromType(GameManager.instance.gameType);
        if (gamerules != null)
        {
            gamerules.ReadMessageValues(message);
        }

        return gamerules;
    }
    #endregion
}
