
import './App.css';
import GoogleMapReact from 'google-map-react';
import addPanel from './add-panel.json' ;
import ReactCircularLoader from 'react-circular-loader';
import { useEffect, useState } from 'react';
import { IAddPanel as IAdPanel } from './panneau';
import axios from 'axios';

function App() {
  const markers: Array<google.maps.Marker> = [];
  const [selectedAd, setSelectedAd] = useState<IAdPanel | null>();
  const [videoToAdd, setVideoToAdd] = useState<any>();
  const [nameToAdd, setNameToAdd] = useState<string>("");
  const [adList, setAdList] = useState<Array<IAdPanel>>([]);

  const getBase64 = (file:File) => {
    return new Promise(resolve => {
      let fileInfo;
      let baseURL = "";
      // Make new FileReader
      let reader = new FileReader();
      reader.readAsDataURL(file);

      reader.onload = () => {
        baseURL = reader.result as string;
        resolve(baseURL);
      };
    });
  }
  const handleSubmit = (containerName:string) => {
    if (!videoToAdd && !nameToAdd) return;

    axios.post('https://localhost:5001/SmartAddsManager/' + containerName, { name: nameToAdd, video: videoToAdd })
      .then(() => 
    {
      let tmpList = [...adList];
const containerIdx = tmpList.findIndex(e=> e.containerName == containerName);
tmpList[containerIdx].containerData.push({dataContent : "", dataName : nameToAdd})
setAdList(tmpList);
}
        );

  };

  const handleDelete = (videoName : string, containerName: string) => {
    axios.delete('https://localhost:5001/SmartAddsManager/' + containerName + "/" + videoName).then(()=>
    {
      let tmpList = [...adList];
  const containerIdx = tmpList.findIndex(e=> e.containerName == containerName);
  const videoIdx = tmpList[containerIdx].containerData.findIndex(e=> e.dataName == videoName);
  tmpList[containerIdx].containerData.splice(videoIdx,1);
  
  setAdList(tmpList);

    });

  };
  
  useEffect(() => {
    axios.get("https://localhost:5001/SmartAddsManager/admin/all-panels").then(
      (response) => 
      {
          let tmpList : Array<IAdPanel>  = response.data;

          tmpList[0].position ={lat : 45.649727001436645, lng: 0.14411331525268523};
          tmpList[1].position={lat : 45.651886936056925, lng: 0.1629531513732907};
          tmpList[2].position={lat : 45.64255662130229, lng: 0.16102196088256804};
          tmpList[3].position={lat : 45.64153649265286, lng: 0.1567304264587399};
          
          setAdList(tmpList);
        })
    .catch((error)=> console.log(error));
  }, []);

  return (
    <div className="App">
      {!!adList && adList.length >0 ?
        <GoogleMapReact
          bootstrapURLKeys={{ key: "" }}
          defaultCenter={{ lat: 45.648377, lng: 0.1562369 }}
          defaultZoom={15}
          options={{ disableDefaultUI: true }}
        
          onGoogleApiLoaded={({ map, maps }) => {
            
            adList.forEach((panel : IAdPanel)=> {
                let marker = new maps.Marker({
                  position: panel.position,
                  map: map,
                  title: panel.containerName,
                })
                marker.addListener("click", ()=> setSelectedAd(panel))
              })
              map.addListener("click", ()=> setSelectedAd(null));
            }
          }
        />
      :
      <p>Chargement...</p>
        // <ReactCircularLoader primaryColor='#0d47a1' diameter='100px' secondaryColor='#e8f4f8' primaryWidth='3px' secondaryWidth='5px'/>
      }
    {selectedAd && 
      <div style={{width: '40%', display:'flex', flexDirection:"column", padding: 20, justifyContent:"space-between"}}>
        <div>
          <div style={{display:'flex', flexDirection:'row', justifyContent:"space-between"}}>
            <h4>Vidéos de {selectedAd.containerName} :</h4>
            <button style={{padding:0, height: 20, width:20, alignSelf:"center"}} onClick={()=> setSelectedAd(null)}>X</button>
          </div>
          <div style={{backgroundColor:"lightgrey", margin: 5, padding: 10, borderRadius:5, maxHeight: '100%', overflow:"auto"}}>
          {selectedAd.containerData.length > 0 ? 
          selectedAd.containerData.map((video, idx:number) => {

            return <div key={video.dataName} 
              style={{
                display:'flex', 
                flexDirection:"row", 
                justifyContent:"space-between", 
                backgroundColor: idx % 2 == 0 ? "white" : "aliceblue",
                margin:5,
                padding:10,
                borderRadius:5
              }}
            >
              <p>{video.dataName}</p> 
              <button style={{padding:0, height: 20, width:20, alignSelf:"center"}} onClick={()=> handleDelete(video.dataName, selectedAd.containerName)}>X</button> 
            </div>
          }) : <p>Pas de vidéos</p>}
          </div>
        </div>
        <div style={{display:'flex', flexDirection:"column"}}>
          <h4>Ajouter une nouvelle vidéo</h4>
          <input type="text" placeholder='Nom de la vidéo' onChange={(e)=> setNameToAdd(e.target.value)} style ={{width: 200}}/>
          <input 
          type="file" 
          accept='video/*'
          style ={{marginBottom: 10, marginTop:5}}
          onChange={(e)=> {
            const newFile = !!e.target.files && e.target.files[0] 
            if (!newFile) return;
            getBase64(newFile)
              .then((result : any) => {
                
                setVideoToAdd(result.split(',')[1])})
              .catch((err:any) => {
                console.log(err);
              });
          }}
          /> 
          <button style ={{alignSelf:"center", width: 60}} onClick={()=> handleSubmit(selectedAd.containerName)}>Valider</button> 
        </div>
      </div>
    }
    </div>
    
  );
}

export default App;

