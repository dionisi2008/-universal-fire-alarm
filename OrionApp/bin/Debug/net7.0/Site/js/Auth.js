document.addEventListener("DOMContentLoaded", () => {
  ExistesSessionBase();
  document.getElementsByTagName("button")[0].addEventListener("click", () => {
    var GetLogin = document.getElementById("Login").value;
    var GetPassword = document.getElementById("Password").value;
    AuthUser(GetLogin, GetPassword);
  });
});

function AuthUser(Login, Password) {
  var ClientAPI = new XMLHttpRequest();
  ClientAPI.open("POST", "/API");
  ClientAPI.send(
    "Auth " + Login + " " + Password + " " + new Date().toLocaleString()
  );
  ClientAPI.addEventListener("loadend", () => {
    if (ClientAPI.responseText.split(" ")[0] == "Auth") {
      if (ClientAPI.responseText.split(" ")[1] != "ERROR") {
        localStorage.setItem("Session", ClientAPI.responseText.split(" ")[1]);
        window.location.href = "/Default.html";
      } else {
        document.getElementsByTagName("label")[0].innerText =
          "Не верный логин или пароль";
      }
    }
  });
  return true;
}

function ExistesSessionBase() {
  if (localStorage.getItem("Session") != null) {
    var WebClient = new XMLHttpRequest();
    WebClient.open("POST", "/API");
    WebClient.send("Session " + localStorage.getItem("Session"));
    WebClient.addEventListener("loadend", () => {
      if (WebClient.responseText == "Session Ok") {
        window.location.href = "/Default.html";
      }
    });
  }
}
