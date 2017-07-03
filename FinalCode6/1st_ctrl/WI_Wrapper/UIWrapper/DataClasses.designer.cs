﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.239
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WF1
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="ParallelTask")]
	public partial class DataClassesDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region 可扩展性方法定义
    partial void OnCreated();
    partial void InsertWaveForm(WaveForm instance);
    partial void UpdateWaveForm(WaveForm instance);
    partial void DeleteWaveForm(WaveForm instance);
    partial void InsertTransmitter(Transmitter instance);
    partial void UpdateTransmitter(Transmitter instance);
    partial void DeleteTransmitter(Transmitter instance);
    partial void InsertAntenna(Antenna instance);
    partial void UpdateAntenna(Antenna instance);
    partial void DeleteAntenna(Antenna instance);
    #endregion
		
		public DataClassesDataContext() : 
				base(global::WF1.Properties.Settings.Default.ParallelTaskConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public DataClassesDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClassesDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClassesDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataClassesDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<WaveForm> WaveForm
		{
			get
			{
				return this.GetTable<WaveForm>();
			}
		}
		
		public System.Data.Linq.Table<Transmitter> Transmitter
		{
			get
			{
				return this.GetTable<Transmitter>();
			}
		}
		
		public System.Data.Linq.Table<Antenna> Antenna
		{
			get
			{
				return this.GetTable<Antenna>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.WaveForm")]
	public partial class WaveForm : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
		private string _Type;
		
		private double _Frequency;
		
		private double _BandWidth;
		
		private System.Nullable<double> _Phase;
		
		private System.Nullable<double> _StartFrequency;
		
		private System.Nullable<double> _EndFrequency;
		
		private System.Nullable<double> _RollOffFactor;
		
		private string _FreChangeRate;
		
		private EntitySet<Transmitter> _Transmitter;
		
		private EntitySet<Antenna> _Antenna;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnTypeChanging(string value);
    partial void OnTypeChanged();
    partial void OnFrequencyChanging(double value);
    partial void OnFrequencyChanged();
    partial void OnBandWidthChanging(double value);
    partial void OnBandWidthChanged();
    partial void OnPhaseChanging(System.Nullable<double> value);
    partial void OnPhaseChanged();
    partial void OnStartFrequencyChanging(System.Nullable<double> value);
    partial void OnStartFrequencyChanged();
    partial void OnEndFrequencyChanging(System.Nullable<double> value);
    partial void OnEndFrequencyChanged();
    partial void OnRollOffFactorChanging(System.Nullable<double> value);
    partial void OnRollOffFactorChanged();
    partial void OnFreChangeRateChanging(string value);
    partial void OnFreChangeRateChanged();
    #endregion
		
		public WaveForm()
		{
			this._Transmitter = new EntitySet<Transmitter>(new Action<Transmitter>(this.attach_Transmitter), new Action<Transmitter>(this.detach_Transmitter));
			this._Antenna = new EntitySet<Antenna>(new Action<Antenna>(this.attach_Antenna), new Action<Antenna>(this.detach_Antenna));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Type", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				if ((this._Type != value))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._Type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Frequency", DbType="Float NOT NULL")]
		public double Frequency
		{
			get
			{
				return this._Frequency;
			}
			set
			{
				if ((this._Frequency != value))
				{
					this.OnFrequencyChanging(value);
					this.SendPropertyChanging();
					this._Frequency = value;
					this.SendPropertyChanged("Frequency");
					this.OnFrequencyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BandWidth", DbType="Float NOT NULL")]
		public double BandWidth
		{
			get
			{
				return this._BandWidth;
			}
			set
			{
				if ((this._BandWidth != value))
				{
					this.OnBandWidthChanging(value);
					this.SendPropertyChanging();
					this._BandWidth = value;
					this.SendPropertyChanged("BandWidth");
					this.OnBandWidthChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Phase", DbType="Float")]
		public System.Nullable<double> Phase
		{
			get
			{
				return this._Phase;
			}
			set
			{
				if ((this._Phase != value))
				{
					this.OnPhaseChanging(value);
					this.SendPropertyChanging();
					this._Phase = value;
					this.SendPropertyChanged("Phase");
					this.OnPhaseChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StartFrequency", DbType="Float")]
		public System.Nullable<double> StartFrequency
		{
			get
			{
				return this._StartFrequency;
			}
			set
			{
				if ((this._StartFrequency != value))
				{
					this.OnStartFrequencyChanging(value);
					this.SendPropertyChanging();
					this._StartFrequency = value;
					this.SendPropertyChanged("StartFrequency");
					this.OnStartFrequencyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EndFrequency", DbType="Float")]
		public System.Nullable<double> EndFrequency
		{
			get
			{
				return this._EndFrequency;
			}
			set
			{
				if ((this._EndFrequency != value))
				{
					this.OnEndFrequencyChanging(value);
					this.SendPropertyChanging();
					this._EndFrequency = value;
					this.SendPropertyChanged("EndFrequency");
					this.OnEndFrequencyChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RollOffFactor", DbType="Float")]
		public System.Nullable<double> RollOffFactor
		{
			get
			{
				return this._RollOffFactor;
			}
			set
			{
				if ((this._RollOffFactor != value))
				{
					this.OnRollOffFactorChanging(value);
					this.SendPropertyChanging();
					this._RollOffFactor = value;
					this.SendPropertyChanged("RollOffFactor");
					this.OnRollOffFactorChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_FreChangeRate", DbType="NVarChar(50)")]
		public string FreChangeRate
		{
			get
			{
				return this._FreChangeRate;
			}
			set
			{
				if ((this._FreChangeRate != value))
				{
					this.OnFreChangeRateChanging(value);
					this.SendPropertyChanging();
					this._FreChangeRate = value;
					this.SendPropertyChanged("FreChangeRate");
					this.OnFreChangeRateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="WaveForm_Transmitter", Storage="_Transmitter", ThisKey="Name", OtherKey="WaveFormName")]
		public EntitySet<Transmitter> Transmitter
		{
			get
			{
				return this._Transmitter;
			}
			set
			{
				this._Transmitter.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="WaveForm_Antenna", Storage="_Antenna", ThisKey="Name", OtherKey="WaveFormName")]
		public EntitySet<Antenna> Antenna
		{
			get
			{
				return this._Antenna;
			}
			set
			{
				this._Antenna.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Transmitter(Transmitter entity)
		{
			this.SendPropertyChanging();
			entity.WaveForm = this;
		}
		
		private void detach_Transmitter(Transmitter entity)
		{
			this.SendPropertyChanging();
			entity.WaveForm = null;
		}
		
		private void attach_Antenna(Antenna entity)
		{
			this.SendPropertyChanging();
			entity.WaveForm = this;
		}
		
		private void detach_Antenna(Antenna entity)
		{
			this.SendPropertyChanging();
			entity.WaveForm = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Transmitter")]
	public partial class Transmitter : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
		private double _RotateX;
		
		private double _RotateY;
		
		private double _RotateZ;
		
		private string _AntennaName;
		
		private string _WaveFormName;
		
		private double _power;
		
		private EntityRef<WaveForm> _WaveForm;
		
		private EntityRef<Antenna> _Antenna;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnRotateXChanging(double value);
    partial void OnRotateXChanged();
    partial void OnRotateYChanging(double value);
    partial void OnRotateYChanged();
    partial void OnRotateZChanging(double value);
    partial void OnRotateZChanged();
    partial void OnAntennaNameChanging(string value);
    partial void OnAntennaNameChanged();
    partial void OnWaveFormNameChanging(string value);
    partial void OnWaveFormNameChanged();
    partial void OnpowerChanging(double value);
    partial void OnpowerChanged();
    #endregion
		
		public Transmitter()
		{
			this._WaveForm = default(EntityRef<WaveForm>);
			this._Antenna = default(EntityRef<Antenna>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RotateX", DbType="Float NOT NULL")]
		public double RotateX
		{
			get
			{
				return this._RotateX;
			}
			set
			{
				if ((this._RotateX != value))
				{
					this.OnRotateXChanging(value);
					this.SendPropertyChanging();
					this._RotateX = value;
					this.SendPropertyChanged("RotateX");
					this.OnRotateXChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RotateY", DbType="Float NOT NULL")]
		public double RotateY
		{
			get
			{
				return this._RotateY;
			}
			set
			{
				if ((this._RotateY != value))
				{
					this.OnRotateYChanging(value);
					this.SendPropertyChanging();
					this._RotateY = value;
					this.SendPropertyChanged("RotateY");
					this.OnRotateYChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RotateZ", DbType="Float NOT NULL")]
		public double RotateZ
		{
			get
			{
				return this._RotateZ;
			}
			set
			{
				if ((this._RotateZ != value))
				{
					this.OnRotateZChanging(value);
					this.SendPropertyChanging();
					this._RotateZ = value;
					this.SendPropertyChanged("RotateZ");
					this.OnRotateZChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AntennaName", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string AntennaName
		{
			get
			{
				return this._AntennaName;
			}
			set
			{
				if ((this._AntennaName != value))
				{
					if (this._Antenna.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnAntennaNameChanging(value);
					this.SendPropertyChanging();
					this._AntennaName = value;
					this.SendPropertyChanged("AntennaName");
					this.OnAntennaNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_WaveFormName", DbType="NVarChar(50)")]
		public string WaveFormName
		{
			get
			{
				return this._WaveFormName;
			}
			set
			{
				if ((this._WaveFormName != value))
				{
					if (this._WaveForm.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnWaveFormNameChanging(value);
					this.SendPropertyChanging();
					this._WaveFormName = value;
					this.SendPropertyChanged("WaveFormName");
					this.OnWaveFormNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_power", DbType="Float NOT NULL")]
		public double power
		{
			get
			{
				return this._power;
			}
			set
			{
				if ((this._power != value))
				{
					this.OnpowerChanging(value);
					this.SendPropertyChanging();
					this._power = value;
					this.SendPropertyChanged("power");
					this.OnpowerChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="WaveForm_Transmitter", Storage="_WaveForm", ThisKey="WaveFormName", OtherKey="Name", IsForeignKey=true)]
		public WaveForm WaveForm
		{
			get
			{
				return this._WaveForm.Entity;
			}
			set
			{
				WaveForm previousValue = this._WaveForm.Entity;
				if (((previousValue != value) 
							|| (this._WaveForm.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._WaveForm.Entity = null;
						previousValue.Transmitter.Remove(this);
					}
					this._WaveForm.Entity = value;
					if ((value != null))
					{
						value.Transmitter.Add(this);
						this._WaveFormName = value.Name;
					}
					else
					{
						this._WaveFormName = default(string);
					}
					this.SendPropertyChanged("WaveForm");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Antenna_Transmitter", Storage="_Antenna", ThisKey="AntennaName", OtherKey="Name", IsForeignKey=true)]
		public Antenna Antenna
		{
			get
			{
				return this._Antenna.Entity;
			}
			set
			{
				Antenna previousValue = this._Antenna.Entity;
				if (((previousValue != value) 
							|| (this._Antenna.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Antenna.Entity = null;
						previousValue.Transmitter.Remove(this);
					}
					this._Antenna.Entity = value;
					if ((value != null))
					{
						value.Transmitter.Add(this);
						this._AntennaName = value.Name;
					}
					else
					{
						this._AntennaName = default(string);
					}
					this.SendPropertyChanged("Antenna");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Antenna")]
	public partial class Antenna : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Name;
		
		private string _Type;
		
		private string _WaveFormName;
		
		private double _MaxGain;
		
		private string _Polarization;
		
		private double _RecerverThreshold;
		
		private double _TransmissionLoss;
		
		private double _VSWR;
		
		private double _Temperature;
		
		private System.Nullable<double> _Radius;
		
		private System.Nullable<double> _BlockageRadius;
		
		private string _ApertureDistribution;
		
		private System.Nullable<double> _EdgeTeper;
		
		private System.Nullable<double> _Length;
		
		private System.Nullable<double> _Pitch;
		
		private EntitySet<Transmitter> _Transmitter;
		
		private EntityRef<WaveForm> _WaveForm;
		
    #region 可扩展性方法定义
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnTypeChanging(string value);
    partial void OnTypeChanged();
    partial void OnWaveFormNameChanging(string value);
    partial void OnWaveFormNameChanged();
    partial void OnMaxGainChanging(double value);
    partial void OnMaxGainChanged();
    partial void OnPolarizationChanging(string value);
    partial void OnPolarizationChanged();
    partial void OnRecerverThresholdChanging(double value);
    partial void OnRecerverThresholdChanged();
    partial void OnTransmissionLossChanging(double value);
    partial void OnTransmissionLossChanged();
    partial void OnVSWRChanging(double value);
    partial void OnVSWRChanged();
    partial void OnTemperatureChanging(double value);
    partial void OnTemperatureChanged();
    partial void OnRadiusChanging(System.Nullable<double> value);
    partial void OnRadiusChanged();
    partial void OnBlockageRadiusChanging(System.Nullable<double> value);
    partial void OnBlockageRadiusChanged();
    partial void OnApertureDistributionChanging(string value);
    partial void OnApertureDistributionChanged();
    partial void OnEdgeTeperChanging(System.Nullable<double> value);
    partial void OnEdgeTeperChanged();
    partial void OnLengthChanging(System.Nullable<double> value);
    partial void OnLengthChanged();
    partial void OnPitchChanging(System.Nullable<double> value);
    partial void OnPitchChanged();
    #endregion
		
		public Antenna()
		{
			this._Transmitter = new EntitySet<Transmitter>(new Action<Transmitter>(this.attach_Transmitter), new Action<Transmitter>(this.detach_Transmitter));
			this._WaveForm = default(EntityRef<WaveForm>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Type", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				if ((this._Type != value))
				{
					this.OnTypeChanging(value);
					this.SendPropertyChanging();
					this._Type = value;
					this.SendPropertyChanged("Type");
					this.OnTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_WaveFormName", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string WaveFormName
		{
			get
			{
				return this._WaveFormName;
			}
			set
			{
				if ((this._WaveFormName != value))
				{
					if (this._WaveForm.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnWaveFormNameChanging(value);
					this.SendPropertyChanging();
					this._WaveFormName = value;
					this.SendPropertyChanged("WaveFormName");
					this.OnWaveFormNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_MaxGain", DbType="Float NOT NULL")]
		public double MaxGain
		{
			get
			{
				return this._MaxGain;
			}
			set
			{
				if ((this._MaxGain != value))
				{
					this.OnMaxGainChanging(value);
					this.SendPropertyChanging();
					this._MaxGain = value;
					this.SendPropertyChanged("MaxGain");
					this.OnMaxGainChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Polarization", DbType="VarChar(50)")]
		public string Polarization
		{
			get
			{
				return this._Polarization;
			}
			set
			{
				if ((this._Polarization != value))
				{
					this.OnPolarizationChanging(value);
					this.SendPropertyChanging();
					this._Polarization = value;
					this.SendPropertyChanged("Polarization");
					this.OnPolarizationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_RecerverThreshold", DbType="Float NOT NULL")]
		public double RecerverThreshold
		{
			get
			{
				return this._RecerverThreshold;
			}
			set
			{
				if ((this._RecerverThreshold != value))
				{
					this.OnRecerverThresholdChanging(value);
					this.SendPropertyChanging();
					this._RecerverThreshold = value;
					this.SendPropertyChanged("RecerverThreshold");
					this.OnRecerverThresholdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_TransmissionLoss", DbType="Float NOT NULL")]
		public double TransmissionLoss
		{
			get
			{
				return this._TransmissionLoss;
			}
			set
			{
				if ((this._TransmissionLoss != value))
				{
					this.OnTransmissionLossChanging(value);
					this.SendPropertyChanging();
					this._TransmissionLoss = value;
					this.SendPropertyChanged("TransmissionLoss");
					this.OnTransmissionLossChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_VSWR", DbType="Float NOT NULL")]
		public double VSWR
		{
			get
			{
				return this._VSWR;
			}
			set
			{
				if ((this._VSWR != value))
				{
					this.OnVSWRChanging(value);
					this.SendPropertyChanging();
					this._VSWR = value;
					this.SendPropertyChanged("VSWR");
					this.OnVSWRChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Temperature", DbType="Float NOT NULL")]
		public double Temperature
		{
			get
			{
				return this._Temperature;
			}
			set
			{
				if ((this._Temperature != value))
				{
					this.OnTemperatureChanging(value);
					this.SendPropertyChanging();
					this._Temperature = value;
					this.SendPropertyChanged("Temperature");
					this.OnTemperatureChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Radius", DbType="Float")]
		public System.Nullable<double> Radius
		{
			get
			{
				return this._Radius;
			}
			set
			{
				if ((this._Radius != value))
				{
					this.OnRadiusChanging(value);
					this.SendPropertyChanging();
					this._Radius = value;
					this.SendPropertyChanged("Radius");
					this.OnRadiusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_BlockageRadius", DbType="Float")]
		public System.Nullable<double> BlockageRadius
		{
			get
			{
				return this._BlockageRadius;
			}
			set
			{
				if ((this._BlockageRadius != value))
				{
					this.OnBlockageRadiusChanging(value);
					this.SendPropertyChanging();
					this._BlockageRadius = value;
					this.SendPropertyChanged("BlockageRadius");
					this.OnBlockageRadiusChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ApertureDistribution", DbType="VarChar(50)")]
		public string ApertureDistribution
		{
			get
			{
				return this._ApertureDistribution;
			}
			set
			{
				if ((this._ApertureDistribution != value))
				{
					this.OnApertureDistributionChanging(value);
					this.SendPropertyChanging();
					this._ApertureDistribution = value;
					this.SendPropertyChanged("ApertureDistribution");
					this.OnApertureDistributionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_EdgeTeper", DbType="Float")]
		public System.Nullable<double> EdgeTeper
		{
			get
			{
				return this._EdgeTeper;
			}
			set
			{
				if ((this._EdgeTeper != value))
				{
					this.OnEdgeTeperChanging(value);
					this.SendPropertyChanging();
					this._EdgeTeper = value;
					this.SendPropertyChanged("EdgeTeper");
					this.OnEdgeTeperChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Length", DbType="Float")]
		public System.Nullable<double> Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				if ((this._Length != value))
				{
					this.OnLengthChanging(value);
					this.SendPropertyChanging();
					this._Length = value;
					this.SendPropertyChanged("Length");
					this.OnLengthChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Pitch", DbType="Float")]
		public System.Nullable<double> Pitch
		{
			get
			{
				return this._Pitch;
			}
			set
			{
				if ((this._Pitch != value))
				{
					this.OnPitchChanging(value);
					this.SendPropertyChanging();
					this._Pitch = value;
					this.SendPropertyChanged("Pitch");
					this.OnPitchChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Antenna_Transmitter", Storage="_Transmitter", ThisKey="Name", OtherKey="AntennaName")]
		public EntitySet<Transmitter> Transmitter
		{
			get
			{
				return this._Transmitter;
			}
			set
			{
				this._Transmitter.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="WaveForm_Antenna", Storage="_WaveForm", ThisKey="WaveFormName", OtherKey="Name", IsForeignKey=true)]
		public WaveForm WaveForm
		{
			get
			{
				return this._WaveForm.Entity;
			}
			set
			{
				WaveForm previousValue = this._WaveForm.Entity;
				if (((previousValue != value) 
							|| (this._WaveForm.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._WaveForm.Entity = null;
						previousValue.Antenna.Remove(this);
					}
					this._WaveForm.Entity = value;
					if ((value != null))
					{
						value.Antenna.Add(this);
						this._WaveFormName = value.Name;
					}
					else
					{
						this._WaveFormName = default(string);
					}
					this.SendPropertyChanged("WaveForm");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Transmitter(Transmitter entity)
		{
			this.SendPropertyChanging();
			entity.Antenna = this;
		}
		
		private void detach_Transmitter(Transmitter entity)
		{
			this.SendPropertyChanging();
			entity.Antenna = null;
		}
	}
}
#pragma warning restore 1591
