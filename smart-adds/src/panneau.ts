export interface IAddPanel {
    
        position : {
            lat : number, lng: number
        },
        containerName : string,
        containerData : Array<
            {
                dataContent: string, 
                dataName:string
            }
        >
} 