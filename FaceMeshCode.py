import cv2
import numpy as np
import mediapipe as mp
import time
import socket
import tkinter as tk
import winsound
from mediapipe.tasks.python.vision import FaceLandmarker, FaceLandmarkerOptions, RunningMode

#sets up camera, fps, and framecount
cap = cv2.VideoCapture("Videos/Video.mp4")

width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

#instantiates and initializes deafult aspect ratio
window_H = 720
window_W = 1280

#calculates the overall screen width and height to display the extracted video
root = tk.Tk()
screen_width = root.winfo_screenwidth()
screen_height = root.winfo_screenheight()
root.destroy()
windowX = int((screen_width - window_W)/2)
windowY = int((screen_height - window_H)/2)

cv2.namedWindow("Image", cv2.WINDOW_NORMAL)
cv2.resizeWindow("Image", window_W, window_H)
cv2.moveWindow("Image", windowX, windowY)
paused = False

#instantiates and initializes the fps by accessing the extracted video's fps (or webcam)
videoFPS = cap.get(cv2.CAP_PROP_FPS)
if videoFPS == 0 or videoFPS is None:
    videoFPS = 30

#function that plays audio from the video (extraction in the terminal required via ffmpe)
#def audio():
    #winsound.PlaySound("Videos/audio.wav", winsound.SND_FILENAME | winsound.SND_ASYNC)
#audio()

prevTime = 0
#meant for detection for video (must also be increasing!)
frame_timestamp = 0

#Instantiates and initializes face landmarking
base_options = mp.tasks.BaseOptions(model_asset_path = "face_landmarker.task")
faceMeshOptions = FaceLandmarkerOptions(
    base_options = base_options,
    running_mode = RunningMode.VIDEO,
    num_faces = 1
)

faceMeshDetector = FaceLandmarker.create_from_options(faceMeshOptions)

#outlines for the face:
head_outline = [
    10, 338, 297, 332, 284,
    251,389, 356, 454, 323,
    361, 288, 397, 365, 379,
    378, 400, 377, 152, 148,
    176, 149, 150, 136, 172,
    58, 132, 93, 234, 127,
    162, 21, 54, 103, 67,
    109, 10
]

right_eye_outline = [
    33, 7, 
    163, 144, 
    145, 153, 
    154, 155,
    133, 173, 
    157, 158, 
    159, 160, 
    161, 246, 
    33
]

left_eye_outline = [
    263, 249, 
    390, 373, 
    374, 380, 
    381, 382, 
    362, 398, 
    384, 385, 
    386, 387, 
    388, 466, 
    263
]

left_eyebrow = [
    55, 65, 52, 
    53, 46, 70, 
    63, 105, 66
]

right_eyebrow = [
    285, 295, 282, 
    283, 276, 300, 
    293, 334, 296
]

outer_lip = [
    61, 146, 91, 181, 84, 17,
    314, 405, 321, 375, 291,
    409, 270, 269, 267, 0,
    37, 39, 40, 185
]

inner_lip = [
    78, 95, 88, 178, 87, 14,
    317, 402, 318, 324, 308,
    415, 310, 311, 312, 13,
    82, 81, 80, 191
]

#instantiates eyeState values for blinking
eyeStates = {
    "left" : {"normalOpen": 0, "smoothOpen": 0},
    "right" : {"normalOpen": 0, "smoothOpen": 0}
}

#function to draw outlines
#note: THIS FUNCTION RELIES ON CURRENT FRAME GLOBALS FROM
#FACELMS
def drawOutlines(arr, arr2):
    for i in arr:
        arr2.append((int(faceLms[i].x * img.shape[1]),
                int(faceLms[i].y * img.shape[0])))
    return  np.array(arr2, dtype = np.int32)
            
#special function to draw irises and simulate blinking
def outlineIris(center, bound, top, bottom, eye):
    state = eyeStates[eye]

    centerX = int(center.x * width)
    centerY = int(center.y * height)

    boundX = int(bound.x * width)
    boundY = int(bound.y * height)

    radius = int(np.sqrt((centerX - boundX)**2 + (centerY - boundY)**2))
                
    #current batch of lines used to simulate "iris squashing": that is,
    #making sure the iris isn't seen when the eye closes.
    currentOpen = top.y - bottom.y
    alpha = 0.1
    state["normalOpen"] = currentOpen if state["normalOpen"] == 0 else (1- alpha) * state["normalOpen"] + alpha * currentOpen
                
    #if-statement is used to ensure that irises don't squash and
    #stretch in extreme values.
    eyeClosedBound = 0.30
    if (currentOpen/state["normalOpen"]) < eyeClosedBound:
        scale = 0.01
        smoothedOpen = 0.01
    else:
        alpha2 = 0.2
        state["smoothOpen"] = currentOpen if state ["smoothOpen"] else (1 - alpha2) * state["smoothOpen"] + alpha2 * currentOpen
        scale = max((state["smoothOpen"]/state["normalOpen"]), 0.01)
                
    radiusX = int(radius)
    radiusY = int(radius * scale)

    minY = int(radius * 0.1)
    maxY = int(radius * 1.2)
    radiusY = max(min(radiusY, maxY), minY)

    #uses ellipse instead of a circle to update the "stretch" values
    cv2.ellipse(img, (centerX, centerY), (radiusX, radiusY), 0,
                0, 360, (0,255,0), 2)
    
#To establish connection with Unity
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ("127.0.0.1", 5067)

while True:
    #allows user to pause the video; limitation is that due to current Python
    #version, audio will still keep playing as winsound does not allow
    #pause and play
    key = cv2.waitKey(1) & 0xFF
    if key == ord(' '):
        paused = not paused
    
    if paused:
        cv2.imshow("Image", canvas)
        continue
   
   
    success, img = cap.read()
    #will loop the video and the audio
    if not success:
        #audio()
        cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
        continue
 
    #switches the python colors from Blue, Green, Red to standard Red, Green, Blue
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    mp_image = mp.Image(image_format = mp.ImageFormat.SRGB, data = imgRGB)
    results = faceMeshDetector.detect_for_video(mp_image, int(frame_timestamp))
    frame_timestamp += 1000/videoFPS   

    #Instantiating data array
    data = []

    #instantiates points and records data
    if results.face_landmarks:
        for faceLms in results.face_landmarks:
            #instantiating landmark id (This will be used to transfer data points)
            landmark_id = []

            #outline arrays
            contour_head = []
            contour_eyes_L = []
            contour_eyes_R = []
            contour_outerMouth = []
            contour_innerMouth = []
            contour_eyebrow_L = []
            contour_eyebrow_R = []

            #draws the outline of the head
            contour_head = drawOutlines(head_outline, contour_head)
            cv2.polylines(img, [contour_head], True, (0, 255, 0), 2)

            #draws eye outlines
            contour_eyes_R = drawOutlines(right_eye_outline, contour_eyes_R)
            cv2.polylines(img, [contour_eyes_R], True, (0, 255, 0), 3)
            contour_eyes_L = drawOutlines(left_eye_outline, contour_eyes_L)
            cv2.polylines(img, [contour_eyes_L], True, (0, 255, 0), 3)

            #draws mouth outlines
            contour_outerMouth = drawOutlines(outer_lip, contour_outerMouth)
            cv2.polylines(img, [contour_outerMouth], True, (0, 255, 0), 2)
            contour_innerMouth = drawOutlines(inner_lip, contour_innerMouth)
            cv2.polylines(img, [contour_innerMouth], True, (0, 255, 0), 2)

            #draws eyebrow outlines
            contour_eyebrow_L = drawOutlines(left_eyebrow, contour_eyebrow_L)
            cv2.polylines(img, [contour_eyebrow_L], True, (0, 255, 0), 2)

            contour_eyebrow_R = drawOutlines(right_eyebrow, contour_eyebrow_R)
            cv2.polylines(img, [contour_eyebrow_R], True, (0, 255, 0), 2)

            #drawing iris
            iris_center_L = faceLms[468]
            iris_center_R = faceLms[473]

            iris_bound_L = faceLms[469]
            iris_bound_R = faceLms[474]

            iris_top_R = faceLms[386]
            iris_top_L = faceLms[159]

            iris_bottom_R = faceLms[374]
            iris_bottom_L = faceLms[145]

            outlineIris(iris_center_L, iris_bound_L, iris_top_L, iris_bottom_L, eye ="left")
            outlineIris(iris_center_R, iris_bound_R, iris_top_R, iris_bottom_R, eye = "right")

            #draws points on the face(all 478)
            for point in faceLms:
                x = (point.x * img.shape[1])
                y = (point.y * img.shape[0])
                cv2.circle(img, (int(x), int(y)), 2, (0, 0, 255), cv2.FILLED)
                landmark_id.append(point)


            #sending data to Unity. Z value is multiplied by width times 35 to get full depth of the face.
            for landmarkPoint in landmark_id:
                pointCoords = [landmarkPoint.x * width, landmarkPoint.y * height, landmarkPoint.z * width * 35]
                data.extend(pointCoords)

            message = f"{width}|{height}|{data}"
            sock.sendto(message.encode(), server_address)
            
    #shows current fps on screen
    currentTime = time.time()
    fps = 1/(currentTime - prevTime) if prevTime != 0 else 0
    prevTime = currentTime
    color = (0, 255, 0) if fps >= 24 else (0, 0, 255)
    cv2.putText(img, f'CurrentFPS: {int(fps)}', (10, 70), cv2.FONT_HERSHEY_PLAIN,
                1, color, 2)
    

    #gives the video or webcam in proper coordinates (so that viewport is optimized)
    finalHeight, finalWidth = img.shape[:2]
    scale = min(window_W/finalWidth, window_H/finalHeight)
    newWindowWidth = int(finalWidth * scale)
    newWindowHeight = int(finalHeight * scale)
    resized = cv2.resize(img, (newWindowWidth, newWindowHeight))
    xOffset = (window_W - newWindowWidth)//2
    yOffset = (window_H - newWindowHeight)//2
    canvas = np.zeros((window_H, window_W, 3), dtype = np.uint8)
    canvas[yOffset: yOffset + newWindowHeight, xOffset: xOffset + newWindowWidth] = resized

    cv2.imshow("Image", canvas)
    cv2.waitKey(1)
   
    