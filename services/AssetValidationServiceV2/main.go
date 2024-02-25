package main

import (
	"AssetValidationV2/validate"
	"bytes"
	"log"
	"sync"
	"time"

	"github.com/gofiber/fiber/v2"
)

type ValidationResponse struct {
	IsValid bool `json:"isValid"`
}

var asyncValidationMux sync.Mutex
var asyncValidationCount = 0

const asyncValidationLimit = 2

func beforeValidation() {
	asyncValidationMux.Lock()
	if asyncValidationCount <= asyncValidationLimit {
		asyncValidationCount++
		asyncValidationMux.Unlock()
		return
	}
	asyncValidationMux.Unlock()

	for {
		time.Sleep(time.Millisecond * 500)
		asyncValidationMux.Lock()
		if asyncValidationCount <= asyncValidationLimit {
			asyncValidationCount++
			asyncValidationMux.Unlock()
			break
		}
		asyncValidationMux.Unlock()
	}
}
func afterValidation() {
	asyncValidationMux.Lock()
	asyncValidationCount--
	asyncValidationMux.Unlock()
}

func main() {
	app := fiber.New()

	app.Get("/", func(c *fiber.Ctx) error {
		return c.SendString("AssetValidationServiceV2 OK")
	})

	app.Post("/api/v1/validate-place", func(c *fiber.Ctx) error {
		beforeValidation()
		defer afterValidation()
		
		body := c.Body()
		log.Println("validating place with size=", len(body))
		nReader := bytes.NewReader(body)
		isOk := validate.IsGameValid(nReader)
		return c.Status(200).JSON(ValidationResponse{
			IsValid: isOk,
		})
	})

	app.Post("/api/v1/validate-item", func(c *fiber.Ctx) error {
		beforeValidation()
		defer afterValidation()

		body := c.Body()
		log.Println("validating item with size=", len(body))
		nReader := bytes.NewReader(body)
		isOk := validate.IsItemValid(nReader)
		return c.Status(200).JSON(ValidationResponse{
			IsValid: isOk,
		})
	})

	log.Fatal(app.Listen(":4300"))
}
