package service

import "websocket/model"
import "time"

type RoomService struct {
	rooms map[string]*model.Room
}

func NewRoomService() *RoomService {
	return &RoomService{
		rooms: make(map[string]*model.Room),
	}
}

func (s *RoomService) CreateRoom(room *model.Room) error {
	s.rooms[room.ID] = room
	room.CreatedAt = time.Now()
	return nil
}

func (s *RoomService) GetRoom(id string) (*model.Room, error) {
	return s.rooms[id], nil
}

func (s *RoomService) UpdateRoom(room *model.Room) error {
	s.rooms[room.ID] = room
	return nil
}

func (s *RoomService) DeleteRoom(id string) error {
	delete(s.rooms, id)
	return nil
}