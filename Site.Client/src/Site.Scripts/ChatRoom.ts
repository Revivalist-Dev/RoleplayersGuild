import * as signalR from '@microsoft/signalr';
import linkifyStr from 'linkify-string';

interface Post {
    characterId: number;
    characterThumbnail: string;
    characterDisplayName: string;
    characterNameClass: string;
    postDateTime: string;
    postContent: string;
    chatPostId: number;
}

interface Room {
    chatRoomId: number;
    contentRating: string;
    chatRoomName: string;
}

document.addEventListener('DOMContentLoaded', () => {
    // This ensures the script runs only after the DOM is fully loaded,
    // and only executes if it finds the specific chat room container on the page.
    const chatRoomContainer = document.querySelector('.card-body .chat-posts-container');
    if (chatRoomContainer) {
        // This is the chat room page, so initialize the chat.
        console.log("ChatRoom.ts: Script loaded and container found. Initializing...");

        const chatRoomIdInput = document.getElementById('chatRoomId') as HTMLInputElement;
        if (!chatRoomIdInput) {
            console.error("ChatRoom.ts: Could not find chatRoomId input. Aborting.");
            return;
        };
        const chatRoomId = parseInt(chatRoomIdInput.value, 10);
        if (isNaN(chatRoomId)) {
            console.error("ChatRoom.ts: ChatRoomId is not a number. Aborting.");
            return;
        }
        console.log(`ChatRoom.ts: ChatRoomId found: ${chatRoomId}`);

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();
        
        console.log("ChatRoom.ts: SignalR HubConnection built.");

        const appendPost = (post: Post) => {
            const noMessagesP = document.querySelector('#ChatOutput .text-muted');
            if (noMessagesP) {
                noMessagesP.remove();
            }

            const postHtml = `
                <div class="Post">
                    <div class="row g-2">
                        <div class="col-2 col-xl-1 text-center">
                            <a href="/Community/Characters/View/${post.characterId}" class="PostAsImage">
                            <img src="${post.characterThumbnail || '/images/Defaults/NewAvatar.png'}" class="chat-avatar" alt="${post.characterDisplayName}" />
                           </div>
                        <div class="col-10 col-xl-11 PostText">
                            <a href="/Community/Characters/View/${post.characterId}" class="fw-bold text-decoration-none ${post.characterNameClass}">${post.characterDisplayName}</a>
                            <small class="text-muted ms-2">${new Date(post.postDateTime).toLocaleString()}</small>
                            <div class="linkify mt-1">${linkifyStr(post.postContent, { target: '_blank' })}</div>
                        </div>
                    </div>
                </div>`;
            const chatOutput = document.getElementById('ChatOutput');
            if (chatOutput) {
                chatOutput.insertAdjacentHTML('beforeend', postHtml);
                chatOutput.scrollTop = chatOutput.scrollHeight;
            }
        };

        connection.on("ReceiveMessage", (post: Post) => {
            appendPost(post);
        });

        connection.on("ReceiveError", (error) => {
            console.error("Chat Error:", error);
            const chatOutput = document.getElementById('ChatOutput');
            const errorMessage = `<div class="alert alert-danger mt-3" role="alert">Error: ${error}</div>`;
            if (chatOutput) {
                chatOutput.insertAdjacentHTML('beforeend', errorMessage);
            }
        });

        connection.onreconnected(connectionId => {
            console.log(`SignalR reconnected. Connection ID: ${connectionId}`);
            connection.invoke("JoinRoom", chatRoomId)
                .catch(err => console.error("Failed to re-join room on reconnected:", err));
        });

        const startSignalRConnection = async () => {
            console.log("ChatRoom.ts: startSignalRConnection() called.");
            try {
                console.log("ChatRoom.ts: Attempting connection.start()...");
                await connection.start();
                console.log("ChatRoom.ts: SignalR Connected.");
                await connection.invoke("JoinRoom", chatRoomId);
                await getInitialPosts(); // Fetch posts immediately after joining
            } catch (err) {
                console.error("SignalR connection error:", err);
            }
        };

        const getInitialPosts = async () => {
            try {
                const response = await fetch(`/api/ChatApi/GetPosts?chatRoomId=${chatRoomId}&lastPostId=0`);
                if (!response.ok) {
                    throw new Error(`Failed to fetch initial posts: ${response.statusText}`);
                }
                const posts: Post[] = await response.json();
                const chatOutput = document.getElementById('ChatOutput');
                if (!chatOutput) return;

                chatOutput.innerHTML = '';

                if (posts.length > 0) {
                    posts.forEach((post: Post) => appendPost(post));
                } else {
                    chatOutput.innerHTML = '<p class="text-center text-muted">No messages yet.</p>';
                }
            } catch (error) {
                console.error("Error fetching initial posts:", error);
                const chatOutput = document.getElementById('ChatOutput');
                if (chatOutput) {
                    chatOutput.innerHTML = '<p class="text-center text-danger">Error loading chat history.</p>';
                }
            }
        };

        const submitPost = async () => {
            const contentInput = document.getElementById('txtPostContent') as HTMLTextAreaElement;
            const characterSelect = document.getElementById('ddlSendAs') as HTMLSelectElement;
            if (!contentInput || !characterSelect) return;

            const content = contentInput.value.trim();
            const characterId = characterSelect.value;
            if (content === '' || characterId === '0' || !characterId) {
                alert("Please enter a message and select a character.");
                return;
            }
            try {
                await connection.invoke("SendMessage", chatRoomId, parseInt(characterId), content);
                contentInput.value = '';
            } catch (error) {
                console.error("Error sending message:", error);
                alert("Failed to send message. Please try again.");
            }
        };

        const getActiveRooms = async () => {
            const listContainer = document.getElementById('ActiveChatRoomList');
            if (!listContainer) return;
            try {
                const response = await fetch('/api/ChatApi/GetActiveRooms');
                if (!response.ok) {
                    throw new Error('Failed to fetch active rooms.');
                }
                const rooms: Room[] = await response.json();
                listContainer.innerHTML = '';
                if (rooms.length > 0) {
                    rooms.forEach((room: Room) => {
                        const roomHtml = `<a href="/Community/Chat-Rooms/Room/${room.chatRoomId}" class="list-group-item list-group-item-action">[${room.contentRating}] - ${room.chatRoomName}</a>`;
                        listContainer.insertAdjacentHTML('beforeend', roomHtml);
                    });
                } else {
                    listContainer.innerHTML = '<p class="p-3 text-muted">No active rooms.</p>';
                }
            } catch (error) {
                console.error('Error fetching active rooms:', error);
                listContainer.innerHTML = '<p class="p-3 text-danger">Could not load rooms.</p>';
            }
        };

        startSignalRConnection();
        getActiveRooms();
        document.getElementById('btnSubmitPost')?.addEventListener('click', submitPost);
        document.getElementById('txtPostContent')?.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                submitPost();
            }
        });

        window.addEventListener('beforeunload', () => {
            if (connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke("LeaveRoom", chatRoomId)
                    .catch(err => console.error("Failed to leave room on unload:", err));
                connection.stop();
            }
        });
    }
});