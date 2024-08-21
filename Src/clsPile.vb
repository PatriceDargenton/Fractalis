
Imports System.Collections.Generic

#Const iClassePilePerso = 0
#Const iClassePileGeneric = 1
' La classe perso. ne marche plus, car elle n'est pas compatible avec les QuadTree
' (les indices de lecture et écriture se mélangent entre les niveaux : cela ne peut
'  marcher qu'avec un seul niveau !?)
'#Const iClassePile = iClassePilePerso
#Const iClassePile = iClassePileGeneric

#Region "Structure PointPile"

' Structure de point optimisée pour la classe de pile suivante
' SizeOf PointPile = 12 octets 
Public Structure PointPile
    Public X, Y As Short

#If iClassePile = iClassePilePerso Then
        Public bDejaTrace As Boolean ' Utile que pour l'algo QuadTree
#End If

    Public Sub New(iX%, iY%)
        X = CShort(iX)
        Y = CShort(iY)
    End Sub
End Structure

#End Region

#If iClassePile = iClassePilePerso Then

Module modPile
    Public bClassePilePerso As Boolean = True
End Module

' Classe pour gérer une pile de pixels
' On arrive au même résultat en utilisant une collection,
'  mais c'est alors 30 fois + lent :
'  Dim collec As New Collection() avec collec.Add(pt)
Public Class ClsPile

    Private m_aptPile() As PointPile ' Pile de pixels à analyser
    Private m_iIndicePileMax%
    Private m_iIndicePileL% ' Lecture
    Private m_iIndicePileE% ' Ecriture

    Public ReadOnly Property bPileVide() As Boolean
        Get
            If m_iIndicePileMax = -1 Then bPileVide = True
        End Get
    End Property

    Public Sub Initialiser()

        m_iIndicePileMax = -1
        m_iIndicePileE = -1
        m_iIndicePileL = -1

    End Sub

    Public Function bEmpiler(iX%, iY%) As Boolean

        'Debug.WriteLine("Empilage : " & iX & ", " & iY & ", max.:" & m_iIndicePileMax)

        bEmpiler = True

        ' Si on atteint la fin de pile, on réutilise les emplacements
        '  au début de la pile
        ' Bug de la version 4 corrigé : m_iIndicePileE + 1 <> m_iIndicePileL
        If m_iIndicePileE + 1 = m_iIndicePileMax And
            m_iIndicePileE + 1 <> m_iIndicePileL Then m_iIndicePileE = -1

        ' Si on rattrape l'indice en lecture de la pile,
        '  ou bien si on dépasse l'indice max. de la pile,
        '  on redimmensionne la pile, et on stocke le point 
        '  en fin de pile
        Dim iIndiceEmpile%
        If (m_iIndicePileE + 1 = m_iIndicePileL OrElse
            m_iIndicePileE + 1 > m_iIndicePileMax) AndAlso
            Not (m_iIndicePileE + 1 = 0 AndAlso
                m_iIndicePileL = 0 AndAlso m_iIndicePileMax = 0) Then
            ' Augmentation dynamique de la taille de la pile
            Try
                m_iIndicePileMax += 1
                If m_iIndicePileMax = 0 Then
                    ReDim m_aptPile(m_iIndicePileMax)
                    m_iIndicePileL = 0
                    m_iIndicePileE = 0
                Else
                    ReDim Preserve m_aptPile(m_iIndicePileMax)
                End If
                iIndiceEmpile = m_iIndicePileMax
            Catch
                bEmpiler = False ' Plus assez de mémoire vive, ça craint !
                Exit Function
            End Try
        Else
            ' On stocke le point à l'indice en écriture
            m_iIndicePileE += 1 ' On veut empiler un nouveau point
            iIndiceEmpile = m_iIndicePileE
        End If

        m_aptPile(iIndiceEmpile) = New PointPile(iX, iY)

    End Function

    Public Function ptDepilerPtPile() As PointPile
        If m_iIndicePileMax = -1 Then Exit Function
        ptDepilerPtPile = m_aptPile(m_iIndicePileL)
    End Function

    Public Function bParcourirPile() As Boolean

        ' On parcours la pile et on renvoit True si on reboucle (ou si vide)
        m_iIndicePileL += 1

        ' Bug de la version 4 corrigé : > et non >=
        If m_iIndicePileL > m_iIndicePileMax Then _
            m_iIndicePileL = 0 : bParcourirPile = True

    End Function

    Public Function bMajBmp() As Boolean

        If ((m_iIndicePileL + 1) Mod 200) = 0 Then bMajBmp = True

    End Function

End Class

#ElseIf iClassePile = iClassePileGeneric Then

Module modPile
    Public bClassePilePerso As Boolean = False
End Module

' Classe pour gérer une pile de pixels
' On arrive au même résultat en utilisant une collection,
'  mais c'est alors 30 fois + lent :
'  Dim collec As New Collection() avec collec.Add(pt)
Public Class ClsPile

    Private m_dTpsDeb As Date
    Private m_aptPile As New Queue(Of PointPile)
    Private m_iCompteur%

    Public ReadOnly Property bPileVide() As Boolean
        Get
            Return (m_aptPile.Count = 0)
            Return False
        End Get
    End Property

    Public Sub Initialiser()

        m_aptPile = New Queue(Of PointPile)
        m_iCompteur = 0
        m_dTpsDeb = Now()

    End Sub

    Public Function bEmpiler(iX%, iY%) As Boolean

        m_aptPile.Enqueue(New PointPile(iX, iY))
        Return True

    End Function

    Public Function ptDepilerPtPile() As PointPile
        Return m_aptPile.Dequeue()
    End Function

    Public Function bParcourirPile() As Boolean
    End Function

    Public Function bMajBmp() As Boolean

        m_iCompteur += 1
        If (m_iCompteur Mod 50) = 0 Then ' Vérifier le tps toutes les 200 itérations
            Dim ts As TimeSpan = Now() - m_dTpsDeb
            Dim rNbSec# = ts.TotalSeconds()
            If rNbSec >= 0.5# Then ' Màj chaque 1/2 sec.
                bMajBmp = True
                m_dTpsDeb = Now
            End If
        End If

    End Function

End Class

#End If