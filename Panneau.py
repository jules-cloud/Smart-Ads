import base64
import requests

response = ""

import base64
import cv2
from time import perf_counter
import tempfile
import numpy as np

panelName = 'panneau-2'

def readVideo(base64Text):
    fh = open("video.mp4", "wb")
    fh.write(base64.b64decode(base64Text))
    fh.close()

    cap = cv2.VideoCapture('video.mp4')
 
    # Check if camera opened successfully
    if (cap.isOpened()== False): 
        print("Error opening video stream or file")
    
    # Read until video is completed
    while(cap.isOpened()):
    # Capture frame-by-frame
        ret, frame = cap.read()
        if ret == True:
    
            # Display the resulting frame
            cv2.imshow('Frame',frame)
    
        # Press Q on keyboard to  exit
            if cv2.waitKey(25) & 0xFF == ord('q'):
                break
        # Break the loop
        else: 
            break
    
    # When everything done, release the video capture object
    cap.release()
    
    # Closes all the frames
    cv2.destroyAllWindows()

    buffer = base64.b64decode(base64Text)
    with tempfile.NamedTemporaryFile() as temp:
        temp.write(buffer)

        cap = cv2.VideoCapture(temp.name)

        frames = cap.get(cv2.CAP_PROP_FRAME_COUNT)
        fps = cap.get(cv2.CAP_PROP_FPS)
        success, frame = cap.read()
        frames = [frame]
        while success:
            success, frame = cap.read()
            frames.append(frame)
        frames = np.stack(frames[:-1])


while(1):
    response = requests.get('https://localhost:5001/SmartAddsManager/video-list/'+panelName,verify=False)
    data = response.json()
    for videoInfo in data :
        readVideo(videoInfo['dataContent'])