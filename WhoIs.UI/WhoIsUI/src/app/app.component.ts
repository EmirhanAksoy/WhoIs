import { Component, OnInit } from '@angular/core';
import { ImageService } from './services/image.service';
import { FaceInfo } from './models/face-info.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  public selectedTab: string = 'faces'
  public faceInfos: FaceInfo[] = [];

  constructor(private imageService: ImageService) {

  }
  ngOnInit(): void {
    this.getFaceInfos()
  }

  getFaceInfos() {
    this.imageService.getFaceInfos().subscribe(response => {
      if (!response?.success) {
        console.error(response);
      }
      this.faceInfos = response?.data || [];
    }, console.error);
  }

}
