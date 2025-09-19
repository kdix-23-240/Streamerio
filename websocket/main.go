package main

import (
	"net/http"
	Handler "websocket/handler"

	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	"github.com/labstack/gommon/log"
)

func main() {
	e := echo.New()

	e.Logger.SetLevel(log.DEBUG)
	e.Use(middleware.Logger())

	// CORS 設定
	e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins: []string{"*"},
		AllowMethods: []string{http.MethodGet, http.MethodPost, http.MethodOptions},
		AllowHeaders: []string{"ngrok-skip-browser-warning", echo.HeaderContentType},
	}))

	e.GET("/", func(c echo.Context) error {
		return c.String(http.StatusOK, "Hello, World!")
	})
	e.GET("/ws-unity", Handler.Default.HandleUnityConnection)
	e.POST("/action", Handler.Default.RelayActionToUnity)
	e.GET("/clients", Handler.Default.ListClients)
	e.Logger.Fatal(e.Start(":8888"))
}
