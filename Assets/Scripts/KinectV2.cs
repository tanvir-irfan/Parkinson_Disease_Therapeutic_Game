//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyBasics {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    /*
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    */
    using Windows.Kinect;
    using ReadWriteCsv;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class KinectV2: /*Window,*/ INotifyPropertyChanged {
        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;


        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        // tanvir.irfan
        public JointType jointType;
        public Point point;


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public KinectV2 ( ) {
            string log = "";
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault ( );
            log += "this.kinectSensor = " + this.kinectSensor.ToString ( ) + ",";

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            log += "this.coordinateMapper = " + this.coordinateMapper.ToString ( ) + ",";


            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader ( );
            log += "this.bodyFrameReader = " + this.bodyFrameReader.ToString ( ) + ",";

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;
            //log += "this.kinectSensor = " + this.kinectSensor.ToString ( ) + ",";

            // open the sensor
            this.kinectSensor.Open ( );
            log += "this.kinectSensor.IsOpen = " + this.kinectSensor.IsOpen + ",";

            // set the status text
            //this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
            //                                              : Properties.Resources.NoSensorStatusText;

            this.StatusText = this.kinectSensor.IsAvailable ? "Available" : "Not Available!";
            log += "this.kinectSensor.IsAvailable = " + this.kinectSensor.IsAvailable + ",";

            log += "this.kinectSensor.KinectCapabilities = " + this.kinectSensor.KinectCapabilities + ",";

            if ( this.bodyFrameReader != null ) {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;                
            }

            writeTest ( log, "Data\\bodyFrameReader.csv" );

        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText {
            get {
                return this.statusText;
            }

            set {
                if ( this.statusText != value ) {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if ( this.PropertyChanged != null ) {
                        this.PropertyChanged ( this, new PropertyChangedEventArgs ( "StatusText" ) );
                    }
                }
            }
        }
        public int count = 0;
        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived ( object sender, BodyFrameArrivedEventArgs e ) {

            string log = "";
            bool dataReceived = false;

            using ( BodyFrame bodyFrame = e.FrameReference.AcquireFrame ( ) ) {
                log += "bodyFrame = " + bodyFrame.ToString() + ",";
                if ( bodyFrame != null ) {
                    if ( this.bodies == null ) {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData ( this.bodies );
                    log += "this.bodies = " + bodies.ToString ( ) + ",";
                    dataReceived = true;
                }
            }

            log += "dataReceived = " + dataReceived + ",";

            string allPoints = "";
            if ( dataReceived ) {
                log += "this.bodies.Length = " + this.bodies.Length + ",";
                foreach ( Body body in this.bodies ) {
                    log += "body.IsTracked = " + body.IsTracked + ",";
                    if ( body.IsTracked ) {                        
                        Dictionary<JointType, Joint> joints = body.Joints;

                        // convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point> ( );                        
                        foreach ( JointType jointType in joints.Keys ) {
                            // sometimes the depth(Z) of an inferred joint may show as negative
                            // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                            CameraSpacePoint position = joints[jointType].Position;
                            if ( position.Z < 0 ) {
                                position.Z = InferredZPositionClamp;
                            }
                            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace ( position );
                            jointPoints[jointType] = new Point ( depthSpacePoint.X, depthSpacePoint.Y );
                            point = jointPoints[jointType];

                            allPoints += jointType.ToString() + " => [ " + point.x + " , " + point.y + " ],";
                        }
                        allPoints = allPoints.Substring ( 0, allPoints.Length - 1 );
                        //writeTest ( allPoints, "Data\\MyAllData.csv" );
                    }
                }
            }

            log = log.Substring ( 0, log.Length - 1 );
            writeTest ( log, String.Format ( "Data\\Reader_FrameArrived{0}.csv", count++ ) );
        }

        public Point getJointDataPoint ( JointType jT ) {
            this.jointType = jT;
            return this.point;         

        }

        void writeTest ( string p, string fileName ) {
            // Write sample data to CSV file
            using ( CsvFileWriter writer = new CsvFileWriter ( fileName ) ) {
                CsvRow row = new CsvRow ( );
                row.Add ( p );
                writer.WriteRow ( row );
            }
        }
        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged ( object sender, IsAvailableChangedEventArgs e ) {
            // on failure, set the status text
            //this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
            //                                                : Properties.Resources.SensorNotAvailableStatusText;
            this.StatusText = this.kinectSensor.IsAvailable ? "Available" : "Not Available!";
        }        
    }

    public struct Point {
        public float x, y;

        public Point ( float px, float py ) {
            x = px;
            y = py;
        }
    }    
}
