package validate

import (
	"fmt"
	"github.com/robloxapi/rbxfile"
	"github.com/robloxapi/rbxfile/rbxl"
	"io"
	"log"
)

func LoadFile(reader io.Reader) (*rbxfile.Root, error) {
	root, warn, err := rbxl.Decoder{}.Decode(reader)
	if err != nil {
		return nil, err
	}
	if warn != nil {
		fmt.Println("[info] read warning:", warn)
	}
	return root, nil
}

func IsItemValid(reader io.Reader) bool {
	file, err := LoadFile(reader)
	if err != nil {
		log.Println("Invalid item file:", err)
		return false
	}
	services := make(map[string]*rbxfile.Instance)
	for _, item := range file.Instances {
		if item.IsService {
			services[item.ClassName] = item
		}
	}
	log.Println("item data", file, services)
	return len(services) == 0
}

func IsGameValid(reader io.Reader) bool {
	file, err := LoadFile(reader)
	if err != nil {
		log.Println("Invalid place file:", err)
		return false
	}
	services := make(map[string]*rbxfile.Instance)
	for _, item := range file.Instances {
		if item.IsService {
			services[item.ClassName] = item
		}
	}
	//fmt.Println("all services", services)
	if _, exists := services["Lighting"]; !exists {
		return false
	}
	_, workspaceExists := services["Workspace"]
	if !workspaceExists {
		return false
	}

	return true
}
