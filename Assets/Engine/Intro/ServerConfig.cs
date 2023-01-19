using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "ServerConfig", order = 1)]
public class ServerConfig : ScriptableObject
{
    [SerializeField] private string serverRoot = "http://127.0.0.1/";
    [SerializeField] private string createUserAPI = "vrbike/create_user.php";
    [SerializeField] private string loginUserAPI = "vrbike/login_user.php";

    public string CreateUserAPI { get { return serverRoot + createUserAPI; } }
    public string LoginUserAPI { get { return serverRoot + loginUserAPI; } }
}
