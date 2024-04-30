import { Component, Input, OnInit } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageService } from '../../services/image.service';
import { FaceInfo } from 'src/app/models/face-info.model';

@Component({
  selector: 'app-face-image',
  templateUrl: './face-image.component.html',
  styleUrls: ['./face-image.component.scss']
})
export class FaceImageComponent implements OnInit {

  @Input() faceInfo!: FaceInfo
  imageUrl: SafeUrl | string = '';
  constructor(private imageService: ImageService,
    private sanitizer: DomSanitizer
  ) { }
  ngOnInit(): void {
    this.getFaceImage();
  }

  getFaceImage() {
    const urlCreator = window.URL;
    this.imageService.getFaceImageAsBlob(this.faceInfo.faceId).subscribe(response => {
      const blob = new Blob([response.body!], { type: response.headers.get('Content-Type')! });
      this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(urlCreator.createObjectURL(blob))
    })
  }

}
