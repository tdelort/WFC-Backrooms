using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    [CreateAssetMenu(fileName = "New Module Prototype", menuName = "WFC/ModulePreBakePrototype")]
    public class ModulePreBakePrototype : ScriptableObject
    {
        // Pour le moment, il n'y a que 8 types de sockets
        // On les defini en les regardant de face :
        // - La première lettre donne le coté (L : left, R : RIGHT, S : SYMETRY)
        // - Le chiffre donne l'epaisseur du mur (0 : vide, 1 : 1/4, 2 : 1/2, 3 : 3/4, 4 : plein)
        // Par exemple, S2 correspond a une face ou il y a un mur de 1/2 d'épaisseur au centre.
        public enum Socket
        {
            S0, S1, S2, S3, S4,
            L0, L1, L2, L3, L4,
            R0, R1, R2, R3, R4
        }

        static readonly Dictionary<Socket, string> socketToString = new Dictionary<Socket, string>()
        {
            {Socket.S0, "S0"}, {Socket.S1, "S1"}, {Socket.S2, "S2"}, {Socket.S3, "S3"}, {Socket.S4, "S4"},
            {Socket.L0, "L0"}, {Socket.L1, "L1"}, {Socket.L2, "L2"}, {Socket.L3, "L3"}, {Socket.L4, "L4"},
            {Socket.R0, "R0"}, {Socket.R1, "R1"}, {Socket.R2, "R2"}, {Socket.R3, "R3"}, {Socket.R4, "R4"}
        };

        public static string SocketToString(Socket socket)
        {
            return socketToString[socket];
        }

        static readonly Dictionary<Socket, Socket> flippedSocket = new Dictionary<Socket, Socket>()
        {
            // Symetric ones
            {Socket.S0, Socket.S0}, {Socket.S1, Socket.S1}, {Socket.S2, Socket.S2}, {Socket.S3, Socket.S3}, {Socket.S4, Socket.S4},
            //Flip L and R
            {Socket.L0, Socket.R0}, {Socket.L1, Socket.R1}, {Socket.L2, Socket.R2}, {Socket.L3, Socket.R3}, {Socket.L4, Socket.R4},
            {Socket.R0, Socket.L0}, {Socket.R1, Socket.L1}, {Socket.R2, Socket.L2}, {Socket.R3, Socket.L3}, {Socket.R4, Socket.L4}
        };

        public static Socket FlipSocket(Socket socket)
        {
            return flippedSocket[socket];
        }

        public GameObject moduleModel;
        public Socket North;
        public Socket East;
        public Socket South;
        public Socket West;
        public bool rotate = true;
        public bool mirror = true;
    }
}